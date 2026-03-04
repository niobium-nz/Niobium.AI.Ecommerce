using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.Ads.Agents;
using OpenAI;

namespace Niobium.Ads.Analyst.Agents
{
    internal class ProductClusterer(OpenAIClient client, ILogger<ProductClusterer> logger)
        : GenericAIAgent<ProductClustererInput, ProductClustererOutput>(client, logger)
    {
        public override string Name => nameof(ProductClusterer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
