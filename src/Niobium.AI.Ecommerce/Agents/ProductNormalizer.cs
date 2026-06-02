using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductNormalizer(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<ProductNormalizer> logger)
        : TypedResponseAgent<ProductNormalizerInput, ProductNormalizerOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductNormalizer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override IEnumerable<AITool> GetTools() => tools.GetWebSearchTools();
    }
}
