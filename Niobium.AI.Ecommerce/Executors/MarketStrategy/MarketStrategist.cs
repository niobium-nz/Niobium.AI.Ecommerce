using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.MarketStrategy;

namespace Niobium.AI.Ecommerce.Executors.MarketStrategy
{
    internal class MarketStrategist(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<MarketStrategist> logger)
        : TypedResponseAgent<MarketStrategyInput, MarketStrategyOutput>(clientFactory, logger)
    {
        public override string Id => nameof(MarketStrategist);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetWebSearchTools(cancellationToken);
    }
}
