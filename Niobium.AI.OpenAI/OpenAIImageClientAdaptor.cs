using OpenAI.Images;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIImageClientAdaptor(ImageClient openAIImageClient) : IImageClient
    {
        public async Task<IEnumerable<Uri>> RunAsync(string conversationID, string prompt, int width, int height, int variantCount = 1, Dictionary<string, Stream>? references = null, CancellationToken cancellationToken = default)
        {
            if (references != null && references.Count > 0)
            {
                throw new NotSupportedException("OpenAI image client does not support multiple image references as input.");
            }

            GeneratedImageSize size = width == 1024 && height == 1024
                ? GeneratedImageSize.W1024xH1024
                : width == 1024 && height == 1536
                    ? GeneratedImageSize.W1024xH1536
                    : width == 1536 && height == 1024 ? GeneratedImageSize.W1536xH1024
                    : throw new NotSupportedException($"Image size not supported by OpenAI image client: {width}x{height}.");

            GeneratedImageCollection images = references == null || references.Count == 0
                ? (GeneratedImageCollection)await openAIImageClient.GenerateImagesAsync(
                    prompt,
                    variantCount,
                    options: new ImageGenerationOptions
                    {
                        OutputFileFormat = GeneratedImageFileFormat.Png,
                        ResponseFormat = GeneratedImageFormat.Uri,
                        Size = size,
                    },
                    cancellationToken: cancellationToken)
                : (GeneratedImageCollection)await openAIImageClient.GenerateImageEditsAsync(
                    references.First().Value,
                    references.First().Key,
                    prompt,
                    variantCount,
                    options: new ImageEditOptions
                    {
                        ResponseFormat = GeneratedImageFormat.Uri,
                        Size = size,
                    },
                    cancellationToken: cancellationToken);
            return images.Select(i => i.ImageUri);
        }
    }
}
