using Microsoft.Extensions.Logging;

namespace Niobium.Ads.Analyst
{
    internal class AnalystWorkflow(
        KeywordsPlanner keywordPlanner,
        AdsDiscoverer adsDiscoverer,
        ProductClusterer productClusterer,
        ProductProfiler productProfiler,
        ProductNormalizer productNormalizer,
        CompetitionScout competitionScout,
        ILogger<AnalystWorkflow> logger)
    {
        private async Task DeployAsync(CancellationToken cancellationToken)
        {
            await keywordPlanner.DeployAsync(cancellationToken);
            await productClusterer.DeployAsync(cancellationToken);
            await productProfiler.DeployAsync(cancellationToken);
            await competitionScout.DeployAsync(cancellationToken);
            await productNormalizer.DeployAsync(cancellationToken);
        }

        public async Task RunAsync(string conversationID, CancellationToken cancellationToken)
        {
            await this.DeployAsync(cancellationToken);

            _ = await competitionScout.RunAsync(conversationID, new CompetitionScoutInput
            {
                Country = "AU",
                Query = "mist pet grooming brush",
                CategoryName = "Pet grooming tool",
                Notes = [
                    "Keep competition research scoped to brushes/combs that DISPENSE mist/spray while brushing; otherwise results will be dominated by standard slicker/rake brushes.",
    "Treat 'steam pet brush' as an alternate market label for similar misting brushes, but sanity-check listings because some are just marketing language for a sprayer.",
    "Separate tool vs consumable: the 'Bathing Solution' is a different competitive set (pet shampoos/cleansers) than the brush hardware.",
    "Avoid broad terms like 'pet hair remover' or 'deshedding tool' in MCP-they pull in vacuums, rollers, rakes, and gloves."
                    ],
                AvoidOrExclusionTerms = [
                   "vacuum",
      "robot vacuum",
      "lint roller",
      "furniture hair remover",
      "carpet cleaner",
      "slicker",
      "pin brush",
      "boar bristle",
      "undercoat rake",
      "deshedding rake",
      "furminator",
      "dematting rake",
      "clipper",
      "trimmer",
      "shampoo",
      "conditioner",
      "wipes",
      "glove",
      "mitt",
      "flea comb"
                    ],
                ProductInterpretations =
                 [
                     new() {
                          InterpretedArchetype= "pet grooming brush with integrated mist/spray dispensing (paired with a gentle bathing/cleansing solution)",
                           Confidence= "High",
                            InterpretedProductType="misting/spray grooming brush for dogs & cats",
                             WhyThisInterpretation = new List<string>
                             {   "Product name includes 'Shower Brush' and brand line 'FreshFur'",
        "Known features explicitly include 'gentle mist + soft bristles' plus coat cleaning/detangling and shedding/itch reduction",
        "Web results for the named product describe a brush that releases a fine mist during brushing" }
                     }
                 ]
            },
            cancellationToken);

            //var targetCountry = "AU";
            //KeywordsPlannerInput input = new()
            //{
            //    CategoryFocus = "Dog Toy",
            //    Country = "US",
            //};

            //var keywords = await keywordPlanner.RunAsync(conversationID, input, cancellationToken);
            //if (keywords.OptimizedKeywords.Count <= 0)
            //{
            //    throw new AgentException($"Failed to get keywords for focus {input.CategoryFocus}");
            //}

            //foreach (var keyword in keywords.OptimizedKeywords)
            //{
            //    AdsDiscovererInput discovererInput = new()
            //    {
            //        Keyword = keyword,
            //        Country = input.Country,
            //    };
            //    var rawAds = await adsDiscoverer.RunAsync(conversationID, discovererInput, cancellationToken);
            //    if (rawAds == null || rawAds.Count <= 0)
            //    {
            //        logger.LogWarning("Failed to get ads for keyword {keyword}", keyword);
            //        continue;
            //    }

            //    ProductClustererOutput clusters = await productClusterer.RunAsync(conversationID, new ProductClustererInput { RawAds = rawAds }, cancellationToken);

            //    foreach (var cluster in clusters.Clusters)
            //    {
            //        if (cluster.LikelyProductName == null)
            //        {
            //            logger.LogWarning("Cluster with id {clusterId} has no likely product name. Skipping this cluster. Cluster details: {@cluster}", cluster.ClusterId, cluster);
            //            continue;
            //        }

            //        var normalizedProduct = await productNormalizer.RunAsync(
            //            conversationID,
            //            new ProductNormalizerInput
            //            {
            //                ProductName = cluster.LikelyProductName,
            //                CategoryName = cluster.CategoryGuess,
            //                KnownFeatures = cluster.KnownFeatures,
            //            },
            //            cancellationToken);

            //        Console.WriteLine();
            //        break;
            //    }

            //Dictionary<ClusteredProduct, List<MetaAd>> productWithAds = [];
            //foreach (var cluster in clusters.Clusters)
            //{
            //    List<MetaAd> adsInCluster = [];
            //    foreach (var ad in rawAds)
            //    {
            //        if (string.IsNullOrWhiteSpace(ad.AdArchiveId))
            //        {
            //            logger.LogWarning("Ad with missing AdArchiveId found. Skipping this ad. Ad details: {@ad}", ad);
            //            continue;
            //        }

            //        if (cluster.AdArchiveIds.Contains(ad.AdArchiveId))
            //        {
            //            adsInCluster.Add(ad);
            //        }
            //    }

            //    productWithAds.Add(cluster, adsInCluster);
            //}

            //foreach (ClusteredProduct product in productWithAds.Keys)
            //{
            //    var adsInCluster = productWithAds[product];
            //    foreach (var ad in adsInCluster)
            //    {
            //        if (string.IsNullOrWhiteSpace(ad.Snapshot?.LinkUrl))
            //        {
            //            logger.LogWarning("Ad with missing Url found. Skipping this ad. Ad details: {@ad}", ad);
            //            continue;
            //        }

            //        var profile = await productProfiler.RunAsync(conversationID, new ProductProfilerInput { LandingPageUrl = ad.Snapshot.LinkUrl }, cancellationToken);
            //    }
            //}
            //}
        }
    }
}
