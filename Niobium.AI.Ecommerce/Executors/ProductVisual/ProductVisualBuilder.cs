using Niobium.AI.Ecommerce.Contracts.ProductVisual;

namespace Niobium.AI.Ecommerce.Executors.ProductVisual
{
    internal class ProductVisualBuilder(IImageClientFactory clientFactory) : GenericImageProducer<ProductVisualBuilderInput>(clientFactory)
    {
        public override string Id => nameof(ProductVisualBuilder);
    }
}
