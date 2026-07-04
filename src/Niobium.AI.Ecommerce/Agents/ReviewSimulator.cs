using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ReviewSimulator(IChatClientFactory clientFactory, Tools.ToolBox tools, ILogger<ReviewSimulator> logger)
        : TypedResponseAgent<ReviewSimulatorInput, ReviewSimulatorOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ReviewSimulator);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override IEnumerable<AITool> GetTools() => [tools.WebSearchTool];
    }
}
