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
            ILogger logger = context.CreateReplaySafeLogger<ProductOnboardingFlow>();
            if (String.IsNullOrWhiteSpace(input.LandingPageUrl))
            {
                logger.LogError("Ad snapshot or LinkUrl is missing in the input. Ending workflow.");
                return null;
            }

            IResponseGenerator<ProductProfilerInput, ProductProfilerOutput> productProfiler = context.GetAgent<ProductProfiler, ProductProfilerInput, ProductProfilerOutput>();
            ProductProfilerOutput profile = await productProfiler.RunAsync(new ProductProfilerInput { LandingPageUrl = input.LandingPageUrl });
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

            ProductOnboardingOutput result = new()
            {
                JobId = input.JobId,
                CandidateId = input.CandidateId,
                ListingId = Guid.NewGuid(),
                MarketingStrategy = marketingStrategy,
                ImageStrategy = imageStrategy,
            };

            string artifactName = $"listing/{result.CandidateId}/{result.ListingId}.json";
            await context.CallActivityAsync(nameof(PublishArtifact), new PublishArtifactInput(artifactName, result));
            logger.LogInformation("Published product onboarding result to artifact storage with name: {ArtifactName}", artifactName);
            return result;
        }
    }
}
