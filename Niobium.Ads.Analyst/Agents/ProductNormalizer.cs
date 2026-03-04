using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.Ads.Agents;
using OpenAI;

namespace Niobium.Ads.Analyst.Agents
{
    internal class ProductNormalizer(OpenAIClient client, McpTools tools, ILogger<ProductNormalizer> logger) : GenericAIAgent<ProductNormalizerInput, ProductNormalizerOutput>(client, logger)
    {
        public override string Name => nameof(ProductNormalizer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);
    }
}
