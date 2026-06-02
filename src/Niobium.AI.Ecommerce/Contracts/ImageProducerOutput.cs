namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ImageProducerOutput
    {
        public required string AssetId { get; init; }

        public required List<Uri> ImageVariants { get; init; } = [];
    }
}
