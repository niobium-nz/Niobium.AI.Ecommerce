using System.Text.Json;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal partial class ProductOnboardingFlow : TaskOrchestrator<ProductOnboardingInput, ProductOnboardingOutput?>
    {
        public override async Task<ProductOnboardingOutput?> RunAsync(TaskOrchestrationContext context, ProductOnboardingInput input)
        {
            if (input.Ad.Snapshot == null || String.IsNullOrWhiteSpace(input.Ad.Snapshot.LinkUrl))
            {
                throw new ArgumentException("Ad snapshot or LinkUrl is missing in the input.");
            }

            ILogger logger = context.CreateReplaySafeLogger<ProductOnboardingFlow>();
            IResponseGenerator<ProductProfilerInput, ProductProfilerOutput> productProfiler = context.GetAgent<ProductProfiler, ProductProfilerInput, ProductProfilerOutput>();
            ProductProfilerOutput profile = await productProfiler.RunAsync(new ProductProfilerInput { LandingPageUrl = input.Ad.Snapshot.LinkUrl });
            if (profile.Product == null || String.IsNullOrWhiteSpace(profile.Product.Name) || profile.Product.KeyClaims.Count <= 0)
            {
                logger.LogError("Competitor product info is missing. Ending workflow.");
                return null;
            }

            IResponseGenerator<MarketStrategyInput, MarketStrategyOutput> marketStrategist = context.GetAgent<MarketStrategist, MarketStrategyInput, MarketStrategyOutput>();
            MarketStrategyOutput marketingStrategy = await marketStrategist.RunAsync(new MarketStrategyInput
            {
                COGSPerUnit = input.Cost.COGSPerUnit,
                ExtraUnitCOGSPerOrder = input.Cost.ExtraUnitCOGSPerOrder,
                CompetitorClaims = profile.Product.KeyClaims,
                CompetitorUsedProductName = profile.Product.Name,
                PaymentProcessingFees = input.Cost.PaymentProcessingFees,
                SalesTax = input.Cost.SalesTax,
                TargetMarketCountry = input.TargetCountry,
                IngredientsOrMaterials = profile.Product.IngredientsOrMaterials,
                CompetitorMarketingHowItWins = profile.Product.HowItWins
            });

            IResponseGenerator<MarketStrategyOutput, ImageStrategyOutput> imageStrategist = context.GetAgent<ImageStrategist, MarketStrategyOutput, ImageStrategyOutput>();
            ImageStrategyOutput imageStrategy = await imageStrategist.RunAsync(marketingStrategy);
            if (!imageStrategy.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogError("Image strategy agent did not return a successful status. Ending workflow.");
                return null;
            }

            List<ImageProducerOutput> landingPageImageReferences = [];
            IResponseGenerator<ImageProducerInput, ImageProducerOutput> imageProducer = context.GetAgent<ImageProducer, ImageProducerInput, ImageProducerOutput>();
            ImageReference productVisualReference = await input.ProductVisual.ToImageReferenceAsync();
            foreach (ImagePromptAsset imagePrompt in imageStrategy.ImagePrompts)
            {
                ImageProducerOutput landingPageImageReference = await imageProducer.RunAsync(new ImageProducerInput
                {
                    AssetId = imagePrompt.AssetId,
                    Form = imagePrompt.ToImageForm(),
                    Prompt = imagePrompt.Prompt,
                    References = [productVisualReference]
                });
                landingPageImageReferences.Add(landingPageImageReference);
            }

            ProductOnboardingOutput result = new()
            {
                JobId = input.JobId,
                CandidateId = input.CandidateId,
                ListingId = Guid.NewGuid(),
                MarketingStrategy = marketingStrategy,
                LandingPageImages = landingPageImageReferences
            };

            string outputPath = $"/artifacts/listing/{result.JobId}/{result.CandidateId}";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            await File.WriteAllTextAsync($"{outputPath}/{result.ListingId}.json", JsonSerializer.Serialize(result));

            return result;
        }
    }
}
