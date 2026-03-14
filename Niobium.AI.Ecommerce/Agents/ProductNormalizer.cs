using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductNormalizer(IChatClientFactory clientFactory, McpTools tools, ILogger<ProductNormalizer> logger) : TypedGenericLanguageAIAgent<ProductNormalizerInput, ProductNormalizerOutput>(clientFactory, logger)
    {
        public override string Name => nameof(ProductNormalizer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override async Task OnResponseGotAsync(string conversationID, ProductNormalizerInput input, ProductNormalizerOutput? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnResponseGotAsync(conversationID, input, output, cancellationToken);
        }
    }
}
