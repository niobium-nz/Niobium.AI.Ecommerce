using Azure.AI.Projects;
using Microsoft.Extensions.Logging;

namespace Niobium.Ads.Analyst
{
    internal class ProductInfoEnricher(AIProjectClient client, ILogger<ProductInfoEnricher> logger)
        : HostedAIAgent<ProductInfoEnricherInput, ProductInfoEnricherOutput>(client, logger)
    {
        public override string Name => nameof(ProductInfoEnricher);
    }
}
