using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class ProductClusterer(AIProjectClient client, ILogger<ProductClusterer> logger)
        : HostedAIAgent<ProductClustererInput, ProductClustererOutput>(client, logger)
    {
        public override string Name => nameof(ProductClusterer);

        protected override ResponseReasoningEffortLevel? Reasoning => ResponseReasoningEffortLevel.Medium;
    }
}
