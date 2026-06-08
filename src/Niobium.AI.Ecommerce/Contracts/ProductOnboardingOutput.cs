namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductOnboardingOutput
    {
        public required Guid CandidateId { get; init; }

        public required MarketStrategyOutput MarketingStrategy { get; set; }

        public required IEnumerable<ImageProducerOutput> LandingPageImages { get; set; }
    }
}
