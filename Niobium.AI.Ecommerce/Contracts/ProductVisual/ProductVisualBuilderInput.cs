namespace Niobium.AI.Ecommerce.Contracts.ProductVisual
{
    internal class ProductVisualBuilderInput : IImageInstruction
    {
        public ImageForm Form { get; set; }

        public List<ImageReference> References { get; set; } = [];
    }
}
