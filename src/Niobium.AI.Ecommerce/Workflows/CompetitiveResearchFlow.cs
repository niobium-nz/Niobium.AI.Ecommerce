using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class CompetitiveResearchFlow : TaskOrchestrator<CompetingProductWithAds, ProductDiscoveryOutput?>
    {
        public override async Task<ProductDiscoveryOutput?> RunAsync(TaskOrchestrationContext context, CompetingProductWithAds input)
        {
            ILogger logger = context.CreateReplaySafeLogger<CompetitiveResearchFlow>();
            if (String.IsNullOrWhiteSpace(input.Product.LikelyProductName))
            {
                logger.LogWarning("Cluster {ClusterId} has no likely product name. Skipping this cluster.", input.Product.ClusterId);
                return null;
            }

            if (input.Product.IsProduct == false)
            {
                logger.LogWarning("Cluster {ClusterId} is not classified as a product. Skipping this cluster.", input.Product.ClusterId);
                return null;
            }

            ProductDiscoveryOutput? nomalizedProduct = await NormalizeAsync(context,
                new CompetitionAnalysisInput
                {
                    Keyword = input.Job.Keyword,
                    SourceCountry = input.Job.SourceCountry,
                    TargetCountry = input.Job.TargetCountry,
                    Product = input
                });

            if (nomalizedProduct == null)
            {
                logger.LogWarning("Product {productName} failed to normalize. Skipping this product.", input.Product.LikelyProductName);
                return null;
            }

            string artifactName = $"discovery/{input.Job.JobId}/{nomalizedProduct.CandidateId}.json";
            await context.CallActivityAsync(nameof(PublishArtifact), new PublishArtifactInput(artifactName, nomalizedProduct));
            logger.LogInformation("Published competitive research result for product {productName} with candidate id {candidateId} to artifact {artifactName}",
                input.Product.LikelyProductName, nomalizedProduct.CandidateId, artifactName);
            return nomalizedProduct;
        }

        private static async Task<ProductDiscoveryOutput?> NormalizeAsync(TaskOrchestrationContext context, CompetitionAnalysisInput input)
        {
            const int ConfidenceThreshold = 6;
            ILogger logger = context.CreateReplaySafeLogger<CompetitiveResearchFlow>();
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
                JobId = input.Product.Job.JobId,
                CandidateId = Guid.NewGuid(),
                Keyword = input.Product.Job.Keyword,
                SourceCountry = input.Product.Job.SourceCountry,
                TargetCountry = input.Product.Job.TargetCountry,
                Product = input.Product.Product,
                Ads = input.Product.Ads,
                CompetitionSignals = competitionSignals
            };
        }

        private static async Task<List<CompetitionScoutOutput>> AnalyzeCompetitionAsync(TaskOrchestrationContext context, CompetitionAnalysisInput input, ProductNormalizerOutput normalizedProduct)
        {
            IResponseGenerator<CompetitionScoutInput, CompetitionScoutOutput> competitionScout = context.GetAgent<CompetitionScout, CompetitionScoutInput, CompetitionScoutOutput>();
            IEnumerable<Task<CompetitionScoutOutput>> tasks = normalizedProduct.KeywordPlan!.RecommendedMcpQueries.Select(k =>
                competitionScout.RunAsync(new CompetitionScoutInput
                {
                    Query = k,
                    Country = input.TargetCountry,
                    CategoryName = input.Product.Product.CategoryGuess,
                    Notes = normalizedProduct.NotesForDownstreamAgent,
                    AvoidOrExclusionTerms = normalizedProduct.KeywordPlan.AvoidOrExclusionTerms,
                    ProductInterpretations = normalizedProduct.ProductInterpretations,
                }));


            ILogger logger = context.CreateReplaySafeLogger<CompetitiveResearchFlow>();
            CompetitionScoutOutput[] competitionSignals = await Task.WhenAll(tasks);
            List<CompetitionScoutOutput> result = [];
            foreach (CompetitionScoutOutput signal in competitionSignals)
            {
                if (!String.IsNullOrWhiteSpace(signal.RawAdsDiscovered.McpError))
                {
                    logger.LogError("MCP error {mcpError} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query.",
                        signal.RawAdsDiscovered.McpError,
                        signal.Query,
                        input.Product.Product.LikelyProductName);
                }
                else
                {
                    result.Add(signal);
                }
            }

            return result;
        }
    }
}
