namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductVisualBuilderInput : IImageInstruction
    {
        public ImageForm Form { get; set; }

        public List<ImageReference> References { get; set; } = [];
    }
}
