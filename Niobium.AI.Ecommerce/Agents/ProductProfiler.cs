using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductProfiler(IChatClientFactory clientFactory, McpTools tools, ILogger<ProductProfiler> logger) : TypedGenericLanguageAIAgent<ProductProfilerInput, ProductProfilerOutput>(clientFactory, logger)
    {
        public override string Name => nameof(ProductProfiler);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override async Task OnRanAsync(string conversationID, ProductProfilerInput input, ProductProfilerOutput? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnRanAsync(conversationID, input, output, cancellationToken);
        }
    }
}
