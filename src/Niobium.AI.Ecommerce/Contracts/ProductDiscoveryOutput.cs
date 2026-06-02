namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductDiscoveryOutput
    {
        public required string SourceCountry { get; init; }

        public required string TargetCountry { get; init; }

        public required string Keyword { get; init; }

        public required CompetingProduct Product { get; init; }

        public required List<MetaAd> Ads { get; init; } = [];

        public List<CompetitionScoutOutput> CompetitionSignals { get; init; } = [];
    }
}
