namespace Niobium.AI.Ecommerce.Contracts.ProductDiscovery
{
    public class ClusteredProduct
    {
        public required string ClusterId { get; set; }

        public required string ClusterLabel { get; set; }

        public string? LandingPageDomain { get; set; }

        public string? LikelyProductName { get; set; }

        public string? CategoryGuess { get; set; }

        public List<string> KnownFeatures { get; set; } = [];

        public required string ClusterConfidence { get; set; }

        public List<string> AdArchiveIds { get; set; } = [];
    }
}
