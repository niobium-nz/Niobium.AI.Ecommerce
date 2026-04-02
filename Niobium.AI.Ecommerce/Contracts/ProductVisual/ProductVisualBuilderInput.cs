namespace Niobium.AI.Ecommerce.Contracts.ProductVisual
{
    internal class ProductVisualBuilderInput : IImageInstruction
    {
        public ImageForm Form { get; set; }

        public Dictionary<string, BinaryData> References { get; set; } = [];
    }
}
