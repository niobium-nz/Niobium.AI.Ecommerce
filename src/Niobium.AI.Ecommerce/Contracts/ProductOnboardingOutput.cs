namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductOnboardingOutput
    {
        public required Guid JobId { get; init; }

        public required Guid CandidateId { get; init; }

        public required Guid ListingId { get; init; }

        public required MarketStrategyOutput MarketingStrategy { get; set; }

        public required ImageStrategyOutput ImageStrategy { get; set; }
    }
}
