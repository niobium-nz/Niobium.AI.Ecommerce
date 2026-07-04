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

            if (input.Ads.All(ad => ad.Snapshot == null || string.IsNullOrWhiteSpace(ad.Snapshot.LinkUrl)))
            {
                logger.LogWarning("Cluster {ClusterId} has no ads with valid landing page link URLs. Skipping this cluster.", input.Product.ClusterId);
                return null;
            }

            bool isDuplicate = await context.CallActivityAsync<bool>(nameof(CheckDuplicateSignal), input.Ads.Where(a => !String.IsNullOrEmpty(a.AdArchiveId)).Select(a => a.AdArchiveId!));
            if (isDuplicate)
            {
                logger.LogWarning("Cluster {ClusterId} contains duplicate candidates. Skipping this cluster.", input.Product.ClusterId);
                return null;
            }

            const int ConfidenceThreshold = 6;
            IResponseGenerator<ProductNormalizerInput, ProductNormalizerOutput> productNormalizer = context.GetAgent<ProductNormalizer, ProductNormalizerInput, ProductNormalizerOutput>();
            ProductNormalizerOutput normalizedProduct = await productNormalizer.RunAsync(new()
            {
                ProductName = input.Product.LikelyProductName!,
                CategoryName = input.Product.CategoryGuess,
                KnownFeatures = input.Product.KnownFeatures,
                Country = input.Job.SourceCountry,
            });

            if (normalizedProduct.Normalization == null || normalizedProduct.Normalization.Confidence0To10 < ConfidenceThreshold)
            {
                logger.LogWarning("Normalized product {productName} has low confidence {confidence}. Skipping competition scouting for this product.",
                    input.Product.LikelyProductName,
                    normalizedProduct.Normalization?.Confidence0To10);
                return null;
            }

            if (normalizedProduct.KeywordPlan == null || normalizedProduct.KeywordPlan.RecommendedMcpQueries.Count <= 0)
            {
                logger.LogError("Normalized product {productName} has no keyword plan. Skipping competition scouting for this product.",
                    input.Product.LikelyProductName);
                return null;
            }

            IResponseGenerator<CompetitionScoutInput, CompetitionScoutOutput> competitionScout = context.GetAgent<CompetitionScout, CompetitionScoutInput, CompetitionScoutOutput>();
            IEnumerable<Task<CompetitionScoutOutput>> tasks = normalizedProduct.KeywordPlan!.RecommendedMcpQueries.Select(k =>
                competitionScout.RunAsync(new CompetitionScoutInput
                {
                    Query = k,
                    Country = input.Job.TargetCountry,
                    CategoryName = input.Product.CategoryGuess,
                    Notes = normalizedProduct.NotesForDownstreamAgent,
                    AvoidOrExclusionTerms = normalizedProduct.KeywordPlan.AvoidOrExclusionTerms,
                    ProductInterpretations = normalizedProduct.ProductInterpretations,
                }));

            CompetitionScoutOutput[] competition = await Task.WhenAll(tasks);
            List<CompetitionScoutOutput> competitionSignals = [];
            foreach (CompetitionScoutOutput signal in competition)
            {
                if (!String.IsNullOrWhiteSpace(signal.RawAdsDiscovered.McpError))
                {
                    logger.LogError("MCP error {mcpError} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query.",
                        signal.RawAdsDiscovered.McpError,
                        signal.Query,
                        input.Product.LikelyProductName);
                }
                else
                {
                    competitionSignals.Add(signal);
                }
            }

            IResponseGenerator<ProductScorerInput, ProductScore> productScorer = context.GetAgent<ProductScorer, ProductScorerInput, ProductScore>();
            ProductScore productScore = await productScorer.RunAsync(new ProductScorerInput
            {
                Product = input.Product,
                Ads = input.Ads,
                CompetitionSignals = competitionSignals
            });

            ProductDiscoveryOutput result = new()
            {
                JobId = input.Job.JobId,
                SignalId = Guid.NewGuid(),
                Keyword = input.Job.Keyword,
                SourceCountry = input.Job.SourceCountry,
                TargetCountry = input.Job.TargetCountry,
                Product = input.Product,
                Ads = input.Ads,
                CompetitionSignals = competitionSignals,
                Score = productScore
            };

            Guid? newPublishedCandidateId = await context.CallActivityAsync<Guid?>(nameof(PublishSignal), result);
            if (newPublishedCandidateId.HasValue)
            {
                logger.LogInformation("Published product candidate {productName} with id {signalId} to database with new candidate id {newCandidateId}",
                    input.Product.LikelyProductName, result.SignalId, newPublishedCandidateId);
            }
            else
            {
                logger.LogWarning("Duplicate found on product candidate {productName} with id {signalId}.",
                    input.Product.LikelyProductName, result.SignalId);
            }

            string artifactName = $"discovery/{input.Job.JobId}/{result.SignalId}.json";
            await context.CallActivityAsync(nameof(PublishArtifact), new PublishArtifactInput(artifactName, result, result.GetType()));
            logger.LogInformation("Published product candidate {productName} with id {signalId} to artifact {artifactName}",
                input.Product.LikelyProductName, result.SignalId, artifactName);
            return result;
        }
    }
}
