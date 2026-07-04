namespace Niobium.AI.Ecommerce.Contracts
{
    internal record ReviewSimulatorInput
    {
        public required string TargetCountry { get; init; }
        public required ProductDetails ProductDetails { get; init; }
        public required CustomerSegment CustomerSegment { get; init; }
    }
}
