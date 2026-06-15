using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductScorer(IChatClientFactory clientFactory, Tools.ToolBox tools, ILogger<ProductScorer> logger)
        : TypedResponseAgent<ProductScorerInput, ProductScore>(clientFactory, logger)
    {
        public override string Id => nameof(ProductScorer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override IEnumerable<AITool> GetTools() => [tools.WebSearchTool];
    }
}
