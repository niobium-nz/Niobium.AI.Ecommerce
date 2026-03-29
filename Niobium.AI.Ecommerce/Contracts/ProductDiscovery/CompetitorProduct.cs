namespace Niobium.AI.Ecommerce.Contracts.ProductDiscovery
{
    internal class CompetitorProduct
    {
        public required ClusteredProduct Product { get; set; }

        public required List<MetaAd> Ads { get; set; } = [];
    }
}
