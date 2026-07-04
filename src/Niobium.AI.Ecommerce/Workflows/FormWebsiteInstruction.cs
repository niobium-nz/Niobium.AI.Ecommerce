using System.Text.Json;
using Microsoft.DurableTask;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class FormWebsiteInstruction : TaskActivity<WebsiteBuildingInput, WebsiteBuildingOutput>
    {
        public override async Task<WebsiteBuildingOutput> RunAsync(TaskActivityContext context, WebsiteBuildingInput input)
        {
            string brandBaseDir = $"/artifacts/brands/{input.BrandCode}";
            string brandJson = await File.ReadAllTextAsync($"{brandBaseDir}/index.json");
            ListingDefinition listing = JsonSerializer.Deserialize<ListingDefinition>(brandJson, SerializationOptions.SnakeCase)
                ?? throw new InvalidOperationException("Brand data could not be loaded.");

            string candidateJson = await File.ReadAllTextAsync($"/artifacts/candidates/{input.SignalId}/{input.CandidateId}.json");
            ProductOnboardingOutput candidate = JsonSerializer.Deserialize<ProductOnboardingOutput>(candidateJson, SerializationOptions.Web)
                ?? throw new InvalidOperationException("Candidate data could not be loaded.");

            CustomerSegment customerSegment = candidate.MarketingStrategy.CustomerSegments.SingleOrDefault(s => s.SegmentNumber == input.CustomerSegmentId)
                ?? throw new InvalidOperationException("Customer segment not found.");
            AngleTrigger triggeredAngle = customerSegment.AngleAndTriggerMatrix.SingleOrDefault(a => a.AngleNumber == input.TriggeredAngleId)
                ?? throw new InvalidOperationException("Triggered angle not found.");

            customerSegment.AngleAndTriggerMatrix.Clear();
            customerSegment.AngleAndTriggerMatrix.Add(triggeredAngle);
            listing.BrandSystem.LogoFile = LocalizeFilePath(brandBaseDir, listing.BrandSystem.LogoFile);
            if (!String.IsNullOrWhiteSpace(listing.TrustSignal.Terms))
            {
                listing.TrustSignal.Terms = LocalizeFilePath(brandBaseDir, listing.TrustSignal.Terms);
            }
            if (!String.IsNullOrWhiteSpace(listing.TrustSignal.ReturnsPolicy))
            {
                listing.TrustSignal.ReturnsPolicy = LocalizeFilePath(brandBaseDir, listing.TrustSignal.ReturnsPolicy);
            }
            if (!String.IsNullOrWhiteSpace(listing.TrustSignal.ShippingPolicy))
            {
                listing.TrustSignal.ShippingPolicy = LocalizeFilePath(brandBaseDir, listing.TrustSignal.ShippingPolicy);
            }
            if (!String.IsNullOrWhiteSpace(listing.TrustSignal.PrivacyPolicy))
            {
                listing.TrustSignal.PrivacyPolicy = LocalizeFilePath(brandBaseDir, listing.TrustSignal.PrivacyPolicy);
            }
            for (int i = 0; i < input.Assets.Length; i++)
            {
                input.Assets[i] = LocalizeFilePath(brandBaseDir, input.Assets[i]);
            }
            listing.BrandSystem.PrimaryColor ??= triggeredAngle.RecommendedWebsitePrimaryColorForThisAngle;
            listing.BrandSystem.SecondaryColor ??= triggeredAngle.RecommendedWebsiteSecondaryColorForThisAngle;
            listing.BrandSystem.AccentColor ??= triggeredAngle.RecommendedWebsiteAccentColorForThisAngle;
            listing.BrandSystem.FontStrategy ??= "Use system fonts only";

            Dictionary<string, WebsiteAsset> assetLibrary = input.Assets.ToDictionary(
                GetFileNameFromUri,
                a => new WebsiteAsset
                {
                    Path = a,
                    MediaRatio = candidate.ImageStrategy.ImagePrompts.FirstOrDefault(img => img.AssetId == GetFileNameFromUri(a))?.Orientation
                });

            WebsiteBuildingOutput result = new()
            {
                BrandSystem = listing.BrandSystem,
                CustomerSegment = customerSegment,
                MobileFirstLandingPagePlan = candidate.MarketingStrategy.MobileFirstLandingPagePlan,
                PricingEconomicsAndOffers = candidate.MarketingStrategy.PricingEconomicsAndOffers,
                ProductDetails = candidate.MarketingStrategy.ProductDetails,
                ShortProductName = listing.ShortProductName ?? triggeredAngle.RecommendedShortProductCode,
                TargetCountry = candidate.TargetCountry,
                TrackingSpec = listing.TrackingSpec,
                VendorIntegration = listing.VendorIntegration,
                TrustSignal = listing.TrustSignal,
                AssetLibrary = assetLibrary,
            };

            return result;
        }

        private static string LocalizeFilePath(string baseDir, string relativePath) => Uri.TryCreate(relativePath, UriKind.Absolute, out Uri? parsedUri)
                ? !parsedUri.IsFile
                    ? throw new InvalidOperationException("Only file URIs are supported for absolute paths.")
                    : parsedUri.LocalPath
                : Path.Combine(baseDir, relativePath);

        private static string GetFileNameFromUri(string uri)
            => Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsedUri)
                ? Path.GetFileNameWithoutExtension(parsedUri.LocalPath)
                : throw new InvalidOperationException($"Invalid URI: {uri}");
    }
}