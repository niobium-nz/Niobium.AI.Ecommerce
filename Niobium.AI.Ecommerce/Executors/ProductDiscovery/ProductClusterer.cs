using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Executors.ProductDiscovery
{
    internal class ProductClusterer(IChatClientFactory clientFactory, ILogger<ProductClusterer> logger)
        : TypedResponseAgent<List<MetaAd>, ProductClustererOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductClusterer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
