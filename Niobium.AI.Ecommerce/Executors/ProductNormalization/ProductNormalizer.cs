using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.ProductNormalization;

namespace Niobium.AI.Ecommerce.Executors.ProductNormalization
{
    internal class ProductNormalizer(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<ProductNormalizer> logger) : TypedResponseAgent<ProductNormalizerInput, ProductNormalizerOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductNormalizer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetWebSearchTools(cancellationToken);

        protected override async Task OnResponseGotAsync(string conversationID, ProductNormalizerInput input, ProductNormalizerOutput? output, CancellationToken cancellationToken)
        {
            if (output != null && output.KeywordPlan != null && output.KeywordPlan.RecommendedMcpQueries.Count > 0)
            {
                output.KeywordPlan.RecommendedMcpQueries = [.. output.KeywordPlan.RecommendedMcpQueries.Select(q => q.Trim().Trim('"').Trim()).Distinct()];
            }
            await base.OnResponseGotAsync(conversationID, input, output, cancellationToken);
        }

        protected override Task OnCleanupAsync(CancellationToken cancellationToken)
            => tools.CleanupPlaywrightTabsAsync(cancellationToken);
    }
}
