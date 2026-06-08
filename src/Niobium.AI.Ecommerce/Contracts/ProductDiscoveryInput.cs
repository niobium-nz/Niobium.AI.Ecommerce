namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductDiscoveryInput
    {
        public required Guid JobId { get; init; }

        public required string SourceCountry { get; init; }

        public required string TargetCountry { get; init; }

        public required string Keyword { get; init; }
    }
}
