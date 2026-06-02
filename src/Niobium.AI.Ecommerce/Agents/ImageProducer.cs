using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ImageProducer(IImageClientFactory clientFactory) : GenericImageProducer<ImageProducerInput, ImageProducerOutput>(clientFactory)
    {
        public override string Id => nameof(ImageProducer);

        protected override int VariantCount => 5;

        protected override Task<string> GetInstructionsAsync(ImageProducerInput input, CancellationToken cancellationToken)
            => Task.FromResult(input.Prompt);

        protected override async Task<ImageProducerOutput> OnResponseGotAsync(ImageProducerInput input, IEnumerable<BinaryData> results, CancellationToken cancellationToken)
        {
            List<Uri> imageVariants = [];
            foreach (BinaryData result in results)
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                await File.WriteAllBytesAsync(tempFilePath, result.ToArray(), cancellationToken);
                imageVariants.Add(new Uri(tempFilePath));
            }

            return new ImageProducerOutput
            {
                AssetId = input.AssetId,
                ImageVariants = imageVariants,
            };
        }
    }
}
