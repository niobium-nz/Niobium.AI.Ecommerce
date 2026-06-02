using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    public class ProductTransformer(IChatClientFactory clientFactory, ILogger<ProductTransformer> logger)
        : TypedResponseAgent<List<MetaAd>, List<CompetingProduct>>(clientFactory, logger)
    {
        public override string Id => nameof(ProductTransformer);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
