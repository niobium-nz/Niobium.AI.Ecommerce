namespace Niobium.AI.Ecommerce.Contracts.ProductDiscovery
{
    internal class ProductDiscoveryInput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public required string Keyword { get; set; }
    }
}
