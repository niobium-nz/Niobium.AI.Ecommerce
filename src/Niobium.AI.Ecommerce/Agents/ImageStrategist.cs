using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ImageStrategist(IChatClientFactory clientFactory, ILogger<ImageStrategist> logger)
        : TypedResponseAgent<MarketStrategyOutput, ImageStrategyOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ImageStrategist);

        protected override ReasoningEffort Reasoning => ReasoningEffort.ExtraHigh;
    }
}
