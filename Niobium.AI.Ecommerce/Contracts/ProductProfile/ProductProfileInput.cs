using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Contracts.ProductProfile
{
    internal class ProductProfileInput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public required string Keyword { get; set; }

        public required ClusteredProduct Product { get; set; }

        public required MetaAd Ad { get; set; }
    }
}
