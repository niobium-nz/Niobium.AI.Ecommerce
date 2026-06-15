namespace Niobium.AI.Ecommerce.Contracts
{
    internal class CompetingProductWithAds
    {
        public required ProductDiscoveryInput Job { get; set; }

        public required CompetingProduct Product { get; set; }

        public required List<MetaAd> Ads { get; set; } = [];
    }
}
