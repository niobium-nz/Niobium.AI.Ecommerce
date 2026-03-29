using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Executors.ProductDiscovery
{
    internal class ProductDiscoveryAggregator() : Executor<ProductClustererOutput, ProductDiscoveryOutput>(nameof(ProductDiscoveryAggregator))
    {
        public override async ValueTask<ProductDiscoveryOutput> HandleAsync(ProductClustererOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            ProductDiscoveryInput userInput = await context.GetUserInput<ProductDiscoveryInput>(cancellationToken);
            List<MetaAd> rawAds = await context.ReadStateAsync<List<MetaAd>>(States.RawAds, States.SharedScope, cancellationToken)
                ?? throw new ExecutorException("Raw ads not found in state.");
            List<CompetitorProduct> products = [];
            foreach (ClusteredProduct cluster in message.Clusters)
            {
                IEnumerable<MetaAd> ads = rawAds.Where(a => a.AdArchiveId != null && cluster.AdArchiveIds.Contains(a.AdArchiveId));
                if (ads.Any())
                {
                    products.Add(new CompetitorProduct
                    {
                        Product = cluster,
                        Ads = [.. ads]
                    });
                }
            }

            return new ProductDiscoveryOutput
            {
                SourceCountry = userInput.SourceCountry,
                TargetCountry = userInput.TargetCountry,
                Keyword = userInput.Keyword,
                Products = products,
            };
        }
    }
}
