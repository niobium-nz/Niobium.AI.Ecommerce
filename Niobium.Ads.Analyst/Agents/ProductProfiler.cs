using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.Ads.Agents;
using OpenAI;

namespace Niobium.Ads.Analyst.Agents
{
    internal class ProductProfiler(OpenAIClient client, McpTools tools, ILogger<ProductProfiler> logger) : GenericAIAgent<ProductProfilerInput, ProductProfilerOutput>(client, logger)
    {
        public override string Name => nameof(ProductProfiler);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);
    }
}
