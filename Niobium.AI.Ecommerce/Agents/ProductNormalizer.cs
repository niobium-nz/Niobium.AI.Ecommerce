using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;
using OpenAI;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductNormalizer(OpenAIClient client, McpTools tools, ILogger<ProductNormalizer> logger) : GenericResponseAIAgent<ProductNormalizerInput, ProductNormalizerOutput>(client, logger)
    {
        public override string Name => nameof(ProductNormalizer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);
    }
}
