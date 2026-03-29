using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.ProductNormalization;

namespace Niobium.AI.Ecommerce.Executors.ProductNormalization
{
    internal class ProductNormalizationAggregator(ILogger<ProductNormalizationAggregator> logger) : Executor<ProductNormalizerOutput, ProductNormalizationOutput?>(nameof(ProductNormalizationAggregator))
    {
        public const int ConfidenceThreshold = 6;

        public override async ValueTask<ProductNormalizationOutput?> HandleAsync(ProductNormalizerOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            ProductNormalizationInput userInput = await context.GetUserInput<ProductNormalizationInput>(cancellationToken);

            if (message.Normalization == null || message.Normalization.Confidence0To10 < ConfidenceThreshold)
            {
                logger.LogWarning("Normalized product {productName} has low confidence {confidence}. Skipping competition scouting for this product.",
                    userInput.Product.Product.LikelyProductName,
                    message.Normalization?.Confidence0To10);
                return null;
            }

            if (message.KeywordPlan == null)
            {
                logger.LogError("Normalized product {productName} has no keyword plan. Skipping competition scouting for this product.",
                    userInput.Product.Product.LikelyProductName);
                return null;
            }

            return new ProductNormalizationOutput
            {
                Keyword = userInput.Keyword,
                SourceCountry = userInput.SourceCountry,
                TargetCountry = userInput.TargetCountry,
                Product = userInput.Product,
                AvoidOrExclusionTerms = message.KeywordPlan.AvoidOrExclusionTerms,
                NormalizedKeywords = message.KeywordPlan.RecommendedMcpQueries,
                CompetitorAnalysisNotes = message.NotesForDownstreamAgent,
                ProductInterpretations = message.ProductInterpretations,
            };
        }
    }
}
