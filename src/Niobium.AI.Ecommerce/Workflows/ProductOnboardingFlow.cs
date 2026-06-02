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
            if (profile.Product == null)
            {
                logger.LogError("Product profiling did not return any product information. Ending workflow.");
                return null;
            }
            if (String.IsNullOrWhiteSpace(profile.Product.Name))
            {
                logger.LogError("Competitor product info is missing. Ending workflow.");
                return null;
            }
            if (profile.Product.KeyClaims.Count <= 0)
            {
                logger.LogError("Competitor product claim is missing. Ending workflow.");
                return null;
            }

            ProductCost cost = cost = await context.WaitForExternalEvent<ProductCost>(nameof(ProductCost));
            IResponseGenerator<MarketStrategyInput, MarketStrategyOutput> marketStrategist = context.GetAgent<MarketStrategist, MarketStrategyInput, MarketStrategyOutput>();
            MarketStrategyOutput marketingStrategy = await marketStrategist.RunAsync(new MarketStrategyInput
            {
                COGSPerUnit = cost.COGSPerUnit,
                ExtraUnitCOGSPerOrder = cost.ExtraUnitCOGSPerOrder,
                CompetitorClaims = profile.Product.KeyClaims,
                CompetitorUsedProductName = profile.Product.Name,
                PaymentProcessingFees = cost.PaymentProcessingFees,
                SalesTax = cost.SalesTax,
                TargetMarketCountry = input.TargetCountry,
                IngredientsOrMaterials = profile.Product.IngredientsOrMaterials,
                CompetitorMarketingHowItWins = profile.Product.HowItWins
            });

            IEnumerable<Task<ImageReference>> competitorProductImageReferences = profile.Product.Images.Select(image => image.ToImageReferenceAsync());

            IResponseGenerator<MarketStrategyOutput, ImageStrategyOutput> imageStrategist = context.GetAgent<ImageStrategist, MarketStrategyOutput, ImageStrategyOutput>();
            ImageStrategyOutput imageStrategy = await imageStrategist.RunAsync(marketingStrategy);
            if (!imageStrategy.Status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogError("Image strategy agent did not return a successful status. Ending workflow.");
                return null;
            }

            IResponseGenerator<ProductVisualBuilderInput, ProductCreativityOutput> productVisualBuilder = context.GetAgent<ProductVisualBuilder, ProductVisualBuilderInput, ProductCreativityOutput>();
            ProductCreativityOutput productVisual = await productVisualBuilder.RunAsync(new ProductVisualBuilderInput
            {
                Form = ImageForm.Square,
                References = [.. await Task.WhenAll(competitorProductImageReferences)]
            });
            if (productVisual.ProductVisual is null || !productVisual.ProductVisual.IsFile)
            {
                logger.LogError("Product visual builder did not return a valid product visual. Ending workflow.");
                return null;
            }

            IResponseGenerator<ImageProducerInput, ImageProducerOutput> imageProducer = context.GetAgent<ImageProducer, ImageProducerInput, ImageProducerOutput>();
            IEnumerable<Task<ImageProducerOutput>> imageProducingTasks = imageStrategy.ImagePrompts.Select(p => imageProducer.RunAsync(new ImageProducerInput
            {
                AssetId = p.AssetId,
                Form = p.ToImageForm(),
                Prompt = p.Prompt,
                References = [new ImageReference
                {
                    Data = BinaryData.FromFile(productVisual.ProductVisual.AbsolutePath, productVisual.MediaType),
                }]
            }));
            ImageProducerOutput[] producedImages = await Task.WhenAll(imageProducingTasks);

            return new ProductOnboardingOutput
            {
                MarketingStrategy = marketingStrategy,
                ProductVisual = productVisual,
                LandingPageImages = producedImages
            };
        }
    }
}
