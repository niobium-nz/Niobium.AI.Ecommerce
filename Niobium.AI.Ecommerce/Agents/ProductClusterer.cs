using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;
using OpenAI;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductClusterer(OpenAIClient client, ILogger<ProductClusterer> logger)
        : GenericResponseAIAgent<ProductClustererInput, ProductClustererOutput>(client, logger)
    {
        public override string Name => nameof(ProductClusterer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
