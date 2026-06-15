using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class ProductDiscoveryFlow : TaskOrchestrator<ProductDiscoveryInput, IEnumerable<CompetingProductWithAds>>
    {
        public override async Task<IEnumerable<CompetingProductWithAds>> RunAsync(TaskOrchestrationContext context, ProductDiscoveryInput input)
        {
            if (String.IsNullOrWhiteSpace(input.Keyword)
                || !Country.TryParse(input.SourceCountry, out _)
                || !Country.TryParse(input.TargetCountry, out _))
            {
                throw new ArgumentException($"Invalid input parameters. Keyword: {input.Keyword}, SourceCountry: {input.SourceCountry}, TargetCountry: {input.TargetCountry}");
            }

            IResponseGenerator<AdsDiscovererInput, List<MetaAd>> adsDiscoverer = context.GetAgent<AdsDiscoverer, AdsDiscovererInput, List<MetaAd>>();
            List<MetaAd> rawAds = await adsDiscoverer.RunAsync(new AdsDiscovererInput
            {
                Keyword = input.Keyword,
                Country = input.SourceCountry
            });

            IResponseGenerator<List<MetaAd>, List<CompetingProduct>> productTransformer = context.GetAgent<ProductTransformer, List<MetaAd>, List<CompetingProduct>>();
            List<CompetingProduct> products = await productTransformer.RunAsync(rawAds);

            ILogger logger = context.CreateReplaySafeLogger<ProductDiscoveryFlow>();

            List<CompetingProductWithAds> result = [];
            foreach (CompetingProduct product in products)
            {
                IEnumerable<MetaAd> ads = rawAds.Where(a => a.AdArchiveId != null && product.AdArchiveIds.Contains(a.AdArchiveId));
                if (!ads.Any())
                {
                    logger.LogWarning("No ads found for cluster {ClusterId} with label {ClusterLabel}. Cluster AdArchiveIds: {AdArchiveIds}",
                        product.ClusterId, product.ClusterLabel, String.Join(", ", product.AdArchiveIds));
                    continue;
                }

                CompetingProductWithAds productWithAds = new()
                {
                    Job = input,
                    Product = product,
                    Ads = [.. ads]
                };
                result.Add(productWithAds);
            }

            string artifactName = $"discovery/{input.JobId}.json";
            await context.CallActivityAsync(nameof(PublishArtifact), new PublishArtifactInput(artifactName, result));
            logger.LogInformation("Published discovery results artifact {ArtifactName} for {Keyword} in {TargetCountry}", artifactName, input.Keyword, input.TargetCountry);

            IEnumerable<Task> tasks = result.Select(r => context.CallSubOrchestratorAsync(nameof(CompetitiveResearchFlow), r));
            logger.LogInformation("Triggered {Count} competitive research flows for {Keyword} in {TargetCountry}", tasks.Count(), input.Keyword, input.TargetCountry);
            await Task.WhenAll(tasks);
            return result;
        }
    }
}
