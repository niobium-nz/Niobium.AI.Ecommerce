using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;
using OpenAI;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductProfiler(OpenAIClient client, McpTools tools, ILogger<ProductProfiler> logger) : GenericResponseAIAgent<ProductProfilerInput, ProductProfilerOutput>(client, logger)
    {
        public override string Name => nameof(ProductProfiler);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override string Model => Models.GPT_5_2;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected async override Task OnRanAsync(string conversationID, ProductProfilerInput input, ProductProfilerOutput? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnRanAsync(conversationID, input, output, cancellationToken);
        }
    }
}
