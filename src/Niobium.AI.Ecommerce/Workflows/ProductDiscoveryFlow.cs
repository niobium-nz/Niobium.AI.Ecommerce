using System.Text.Json;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class ProductDiscoveryFlow : TaskOrchestrator<ProductDiscoveryInput, IEnumerable<ProductDiscoveryOutput>>
    {
        public override async Task<IEnumerable<ProductDiscoveryOutput>> RunAsync(TaskOrchestrationContext context, ProductDiscoveryInput input)
        {
            if (String.IsNullOrWhiteSpace(input.Keyword)
                || !Country.TryParse(input.SourceCountry, out _)
                || !Country.TryParse(input.TargetCountry, out _))
            {
                throw new ArgumentException($"Invalid input parameters. Keyword: {input.Keyword}, SourceCountry: {input.SourceCountry}, TargetCountry: {input.TargetCountry}");
            }

            ILogger logger = context.CreateReplaySafeLogger<ProductDiscoveryFlow>();
            IEnumerable<CompetingProductWithAds> products = await DiscoverProductsAsync(context, new AdsDiscovererInput
            {
                Keyword = input.Keyword,
                Country = input.SourceCountry
            });

            List<ProductDiscoveryOutput> result = [];
            foreach (CompetingProductWithAds product in products)
            {
                ProductDiscoveryOutput? nomalizedProduct = await NormalizeAsync(context,
                    input.JobId,
                    new CompetitionAnalysisInput
                    {
                        Keyword = input.Keyword,
                        SourceCountry = input.SourceCountry,
                        TargetCountry = input.TargetCountry,
                        Product = product
                    });

                if (nomalizedProduct == null)
                {
                    logger.LogWarning("Product {productName} failed to normalize. Skipping this product.", product.Product.LikelyProductName);
                }
                else
                {
                    result.Add(nomalizedProduct);

                    string outputDir = $"/artifacts/discovery/{input.JobId}";
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    string json = JsonSerializer.Serialize(nomalizedProduct);
                    await File.WriteAllTextAsync($"{outputDir}/{nomalizedProduct.CandidateId}.json", json);
                }
            }

            return result;
        }

        private static async Task<IEnumerable<CompetingProductWithAds>> DiscoverProductsAsync(TaskOrchestrationContext context, AdsDiscovererInput input)
        {
            IResponseGenerator<AdsDiscovererInput, List<MetaAd>> adsDiscoverer = context.GetAgent<AdsDiscoverer, AdsDiscovererInput, List<MetaAd>>();
            List<MetaAd> rawAds = await adsDiscoverer.RunAsync(input);

            IResponseGenerator<List<MetaAd>, List<CompetingProduct>> productTransformer = context.GetAgent<ProductTransformer, List<MetaAd>, List<CompetingProduct>>();
            List<CompetingProduct> products = await productTransformer.RunAsync(rawAds);

            ILogger logger = context.CreateReplaySafeLogger<ProductDiscoveryFlow>();
            List<CompetingProductWithAds> results = [];
            foreach (CompetingProduct product in products)
            {
                if (String.IsNullOrWhiteSpace(product.LikelyProductName))
                {
                    logger.LogWarning("Cluster {ClusterId} has no likely product name. Skipping this cluster.", product.ClusterId);
                    continue;
                }

                IEnumerable<MetaAd> ads = rawAds.Where(a => a.AdArchiveId != null && product.AdArchiveIds.Contains(a.AdArchiveId));
                if (!ads.Any())
                {
                    logger.LogWarning("No ads found for cluster {ClusterId} with label {ClusterLabel}. Cluster AdArchiveIds: {AdArchiveIds}",
                        product.ClusterId, product.ClusterLabel, String.Join(", ", product.AdArchiveIds));
                    continue;
                }

                results.Add(new CompetingProductWithAds
                {
                    Product = product,
                    Ads = [.. ads]
                });
            }

            return results;
        }

        private static async Task<ProductDiscoveryOutput?> NormalizeAsync(TaskOrchestrationContext context, Guid jobId, CompetitionAnalysisInput input)
        {
            const int ConfidenceThreshold = 6;
            ILogger logger = context.CreateReplaySafeLogger<ProductDiscoveryFlow>();
            IResponseGenerator<ProductNormalizerInput, ProductNormalizerOutput> productNormalizer = context.GetAgent<ProductNormalizer, ProductNormalizerInput, ProductNormalizerOutput>();
            ProductNormalizerOutput normalizedProduct = await productNormalizer.RunAsync(new()
            {
                ProductName = input.Product.Product.LikelyProductName!,
                CategoryName = input.Product.Product.CategoryGuess,
                KnownFeatures = input.Product.Product.KnownFeatures,
                Country = input.SourceCountry,
            });

            if (normalizedProduct.Normalization == null || normalizedProduct.Normalization.Confidence0To10 < ConfidenceThreshold)
            {
                logger.LogWarning("Normalized product {productName} has low confidence {confidence}. Skipping competition scouting for this product.",
                    input.Product.Product.LikelyProductName,
                    normalizedProduct.Normalization?.Confidence0To10);
                return null;
            }

            if (normalizedProduct.KeywordPlan == null || normalizedProduct.KeywordPlan.RecommendedMcpQueries.Count <= 0)
            {
                logger.LogError("Normalized product {productName} has no keyword plan. Skipping competition scouting for this product.",
                    input.Product.Product.LikelyProductName);
                return null;
            }

            List<CompetitionScoutOutput> competitionSignals = await AnalyzeCompetitionAsync(context, input, normalizedProduct);
            return new ProductDiscoveryOutput
            {
                JobId = jobId,
                CandidateId = Guid.NewGuid(),
                Keyword = input.Keyword,
                SourceCountry = input.SourceCountry,
                TargetCountry = input.TargetCountry,
                Product = input.Product.Product,
                Ads = input.Product.Ads,
                CompetitionSignals = competitionSignals
            };
        }

        private static async Task<List<CompetitionScoutOutput>> AnalyzeCompetitionAsync(TaskOrchestrationContext context, CompetitionAnalysisInput input, ProductNormalizerOutput normalizedProduct)
        {
            ILogger logger = context.CreateReplaySafeLogger<ProductDiscoveryFlow>();
            List<CompetitionScoutOutput> competitionSignals = [];
            foreach (string keyword in normalizedProduct.KeywordPlan!.RecommendedMcpQueries)
            {
                IResponseGenerator<CompetitionScoutInput, CompetitionScoutOutput> competitionScout = context.GetAgent<CompetitionScout, CompetitionScoutInput, CompetitionScoutOutput>();
                CompetitionScoutOutput signal = await competitionScout.RunAsync(new CompetitionScoutInput
                {
                    Query = keyword,
                    Country = input.TargetCountry,
                    CategoryName = input.Product.Product.CategoryGuess,
                    Notes = normalizedProduct.NotesForDownstreamAgent,
                    AvoidOrExclusionTerms = normalizedProduct.KeywordPlan.AvoidOrExclusionTerms,
                    ProductInterpretations = normalizedProduct.ProductInterpretations,
                });

                if (!String.IsNullOrWhiteSpace(signal.RawAdsDiscovered.McpError))
                {
                    logger.LogError("MCP error {mcpError} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query.",
                        signal.RawAdsDiscovered.McpError,
                        signal.Query,
                        input.Product.Product.LikelyProductName);
                }
                else
                {
                    competitionSignals.Add(signal);
                }
            }

            return competitionSignals;
        }
    }
}
