using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class MarketStrategist(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<MarketStrategist> logger)
        : TypedResponseAgent<MarketStrategyInput, MarketStrategyOutput>(clientFactory, logger)
    {
        public override string Id => nameof(MarketStrategist);

        protected override ReasoningEffort Reasoning => ReasoningEffort.ExtraHigh;

        protected override IEnumerable<AITool> GetTools() => tools.GetWebSearchTools();

        //https://github.com/bear2u/my-skills/blob/master/skills/landing-page-guide-v2/SKILL.md
    }
}
