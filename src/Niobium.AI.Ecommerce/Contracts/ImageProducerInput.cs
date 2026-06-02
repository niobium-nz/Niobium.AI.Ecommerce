namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ImageProducerInput : IImageInstruction
    {
        public required string AssetId { get; init; }
        
        public required ImageForm Form { get; init; }

        public required List<ImageReference> References { get; init; } = [];

        public required string Prompt { get; init; }
    }
}
