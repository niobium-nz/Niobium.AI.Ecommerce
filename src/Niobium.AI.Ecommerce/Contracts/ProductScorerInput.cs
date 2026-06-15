namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductScorerInput
    {
        public required CompetingProduct Product { get; init; }

        public required List<MetaAd> Ads { get; init; } = [];

        public List<CompetitionScoutOutput> CompetitionSignals { get; init; } = [];
    }
}
