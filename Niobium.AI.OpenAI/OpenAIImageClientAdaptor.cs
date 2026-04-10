using System.Drawing;
using Microsoft.Extensions.AI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIImageClientAdaptor(IImageGenerator imageGenerator) : IImageClient
    {
        private static readonly Size Square = new(1024, 1024);
        private static readonly Size Vertical = new(1024, 1536);
        private static readonly Size Horizontal = new(1536, 1024);

        public async Task<IEnumerable<BinaryData>> RunAsync(string conversationID, string prompt, int width, int height, int variantCount = 1, List<ImageReference>? references = null, CancellationToken cancellationToken = default)
        {
            if (references != null && references.Count > 1)
            {
                throw new NotSupportedException("OpenAI image client does not support multiple image references as input.");
            }

            Size size = width == 1024 && height == 1024
                ? Square
                : width == 1024 && height == 1536
                    ? Vertical
                    : width == 1536 && height == 1024 ? Horizontal
                    : throw new NotSupportedException($"Image size not supported by OpenAI image client: {width}x{height}.");

            ImageGenerationOptions options = new()
            {
                Count = variantCount,
                ImageSize = size,
                MediaType = "image/png",
            };

            ImageGenerationResponse images;
            if (references == null || references.Count == 0)
            {
                images = await imageGenerator.GenerateImagesAsync(
                    prompt,
                    options: options,
                    cancellationToken: cancellationToken);
            }
            else
            {
                ImageReference reference = references.Single();
                images = await imageGenerator.EditImageAsync(
                    new DataContent(reference.Data, reference.MediaType),
                    prompt,
                    options: options,
                    cancellationToken: cancellationToken);
            }

            return images.Contents.Where(c => c is DataContent).Select(c => BinaryData.FromBytes(((DataContent)c).Data));
        }
    }
}
