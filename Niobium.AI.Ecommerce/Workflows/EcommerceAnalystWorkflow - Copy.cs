//using System.Text.Json;
//using Microsoft.Agents.AI.Workflows;
//using Microsoft.Extensions.Logging;
//using Niobium.AI.Ecommerce.Contracts;
//using Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis;
//using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;
//using Niobium.AI.Ecommerce.Contracts.ProductProfile;
//using Niobium.AI.Ecommerce.Executors.CompetitorAnalysis;
//using Niobium.AI.Ecommerce.Executors.MarketResearch;
//using Niobium.AI.Ecommerce.Executors.ProductDiscovery;
//using Niobium.AI.Ecommerce.Executors.ProductNormalization;
//using Niobium.AI.Ecommerce.Executors.ProductProfile;

//namespace Niobium.AI.Ecommerce.Workflows
//{
//    internal class EcommerceAnal1ystWorkflow(
//        KeywordsExpander keywordPlanner,
//        AdsDiscoverer adsDiscoverer,
//        ProductClusterer productClusterer,
//        ProductProfiler productProfiler,
//        ProductNormalizer productNormalizer,
//        CompetitionScout competitionScout,
//        ILogger<EcommerceAnalystWorkflow> logger) : IWorkflow
//    {
//        public string Id => nameof(EcommerceAnalystWorkflow);

//        public string Render() => throw new NotImplementedException();

//        public async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
//        {
//            List<Exception> exceptions = [];

//            do
//            {
//                var focus = "Dog Toy";
//                var sourceCountry = "US";
//                var targetCountry = "AU";

//                var keywords = await keywordPlanner.GetResponseAsync(conversationID,
//                    new KeywordsExpanderInput
//                    {
//                        CategoryFocus = focus,
//                        SourceCountry = sourceCountry,
//                    },
//                    cancellationToken);
//                if (keywords.OptimizedKeywords.Count <= 0)
//                {
//                    exceptions.Add(new ExecutorException($"Failed to get keywords for focus {focus}"));
//                    break;
//                }

//                foreach (var keyword in keywords.OptimizedKeywords)
//                {
//                    var rawAds = await adsDiscoverer.GetResponseAsync(
//                        conversationID,
//                        new AdsDiscovererInput
//                        {
//                            Keyword = keyword,
//                            Country = sourceCountry,
//                        },
//                        cancellationToken);
//                    if (rawAds == null || rawAds.Count <= 0)
//                    {
//                        exceptions.Add(new ExecutorException($"Failed to get ads for keyword {keyword}"));
//                        continue;
//                    }

//                    ProductClustererOutput clusters = await productClusterer.GetResponseAsync(
//                        conversationID,
//                        new ProductClustererInput { RawAds = rawAds },
//                        cancellationToken);

//                    foreach (var cluster in clusters.Clusters)
//                    {
//                        if (cluster.LikelyProductName == null)
//                        {
//                            logger.LogWarning("Cluster with id {clusterId} has no likely product name. Skipping this cluster. Cluster details: {@cluster}", cluster.ClusterId, cluster);
//                            continue;
//                        }

//                        var normalizedProduct = await productNormalizer.GetResponseAsync(
//                            conversationID,
//                            new ProductNormalizerInput
//                            {
//                                ProductName = cluster.LikelyProductName,
//                                CategoryName = cluster.CategoryGuess,
//                                KnownFeatures = cluster.KnownFeatures,
//                                Country = sourceCountry,
//                            },
//                            cancellationToken);

//                        if (normalizedProduct.Normalization?.Confidence0To10 < 7)
//                        {
//                            logger.LogWarning("Normalized product {productName} has low confidence {confidence}. Skipping competition scouting for this product. Normalized product details: {@normalizedProduct}",
//                                cluster.LikelyProductName,
//                                normalizedProduct.Normalization?.Confidence0To10,
//                                normalizedProduct);
//                            continue;
//                        }

//                        if (normalizedProduct.KeywordPlan == null)
//                        {
//                            logger.LogError("Normalized product {productName} has no keyword plan. Skipping competition scouting for this product. Normalized product details: {@normalizedProduct}",
//                                cluster.LikelyProductName,
//                                normalizedProduct);
//                            continue;
//                        }

//                        bool shouldProceed = true;
//                        foreach (var competitiveSearchQuery in normalizedProduct.KeywordPlan.RecommendedMcpQueries)
//                        {
//                            CompetitionScoutOutput report = await competitionScout.GetResponseAsync(
//                                conversationID,
//                                new CompetitionScoutInput
//                                {
//                                    Query = competitiveSearchQuery,
//                                    CategoryName = cluster.CategoryGuess,
//                                    Country = targetCountry,
//                                    AvoidOrExclusionTerms = normalizedProduct.KeywordPlan.AvoidOrExclusionTerms,
//                                    Notes = normalizedProduct.NotesForDownstreamAgent,
//                                    ProductInterpretations = normalizedProduct.ProductInterpretations,
//                                },
//                                cancellationToken);

//                            if (!String.IsNullOrWhiteSpace(report.RawAdsDiscovered.McpError))
//                            {
//                                logger.LogError("MCP error {mcpError} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query. Competition scout output details: {@report}",
//                                    report.RawAdsDiscovered.McpError,
//                                    competitiveSearchQuery,
//                                    cluster.LikelyProductName,
//                                    report);
//                                continue;
//                            }

//                            if (report.CompetitionSignal.Confidence0To10 < 6)
//                            {
//                                logger.LogWarning("Low competition signal confidence {confidence} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query. Competition scout output details: {@report}",
//                                    report.CompetitionSignal.Confidence0To10,
//                                    competitiveSearchQuery,
//                                    cluster.LikelyProductName,
//                                    report);
//                                continue;
//                            }

//                            if (report.CompetitionSignal.Rating0To10 >= 6)
//                            {
//                                logger.LogInformation("High competition signal rating {rating} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping further competitive search queries for this product. Competition scout output details: {@report}",
//                                    report.CompetitionSignal.Rating0To10,
//                                    competitiveSearchQuery,
//                                    cluster.LikelyProductName,
//                                    report);
//                                shouldProceed = false;
//                                break;
//                            }
//                        }

//                        if (!shouldProceed)
//                        {
//                            logger.LogInformation("Skipping further processing for product {productName} due to high competition signal. Cluster details: {@cluster}", cluster.LikelyProductName, cluster);
//                            break;
//                        }

//                        foreach (var adArchiveId in cluster.AdArchiveIds)
//                        {
//                            MetaAd? ad = rawAds.FirstOrDefault(a => a.AdArchiveId == adArchiveId);
//                            if (String.IsNullOrWhiteSpace(ad?.Snapshot?.LinkUrl))
//                            {
//                                logger.LogWarning("Cannot find ad landing page url. Skipping this ad. Ad Archive: {adArchiveId}", adArchiveId);
//                                continue;
//                            }

//                            ProductProfilerOutput profile = await productProfiler.GetResponseAsync(
//                                conversationID,
//                                new ProductProfilerInput { LandingPageUrl = ad.Snapshot.LinkUrl },
//                                cancellationToken);
//                            await File.WriteAllTextAsync(@"C:\Users\Wen\Desktop\profile.json", JsonSerializer.Serialize(profile, SerializationOptions.SnakeCase));
//                        }
//                    }
//                }
//            } while (false);

//            if (exceptions.Count > 0)
//            {
//                var aggregateException = new AggregateException(exceptions);
//                logger.LogError(aggregateException, "Workflow completed with {exceptionCount} errors. Throwing aggregate exception. Exceptions: {@exceptions}", exceptions.Count, exceptions);
//                throw aggregateException;
//            }

//            return String.Empty;
//        }
//    }
//}
