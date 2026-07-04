namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductOnboardingInput
    {
        public required Guid JobId { get; init; }

        public required Guid SignalId { get; init; }

        public required string TargetCountry { get; set; }

        public required string LandingPageUrl { get; set; }

        public required ProductCost Cost { get; set; }
    }
}
