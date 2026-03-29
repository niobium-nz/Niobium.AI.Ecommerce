namespace Niobium.AI.Ecommerce.Contracts.ProductDiscovery
{
    internal class ProductDiscoveryOutput : ProductDiscoveryInput
    {
        public List<CompetitorProduct> Products { get; set; } = [];
    }
}
