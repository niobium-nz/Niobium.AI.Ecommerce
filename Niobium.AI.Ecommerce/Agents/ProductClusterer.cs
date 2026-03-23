using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductClusterer(IChatClientFactory clientFactory, ILogger<ProductClusterer> logger)
        : TypedResponseAgent<ProductClustererInput, ProductClustererOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductClusterer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
