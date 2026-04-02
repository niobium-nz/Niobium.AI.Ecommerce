using OpenAI.Images;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIImageClientAdaptor(ImageClient openAIImageClient) : IImageClient
    {
        public async Task<IEnumerable<BinaryData>> RunAsync(string conversationID, string prompt, int width, int height, int variantCount = 1, Dictionary<string, BinaryData>? references = null, CancellationToken cancellationToken = default)
        {
            if (references != null && references.Count > 1)
            {
                throw new NotSupportedException("OpenAI image client does not support multiple image references as input.");
            }

            GeneratedImageSize size = width == 1024 && height == 1024
                ? GeneratedImageSize.W1024xH1024
                : width == 1024 && height == 1536
                    ? GeneratedImageSize.W1024xH1536
                    : width == 1536 && height == 1024 ? GeneratedImageSize.W1536xH1024
                    : throw new NotSupportedException($"Image size not supported by OpenAI image client: {width}x{height}.");

            GeneratedImageCollection images;
            if (references == null || references.Count == 0)
            {
                images = await openAIImageClient.GenerateImagesAsync(
                    prompt,
                    variantCount,
                    options: new ImageGenerationOptions
                    {
                        OutputFileFormat = GeneratedImageFileFormat.Png,
                        ResponseFormat = GeneratedImageFormat.Uri,
                        Size = size,
                    },
                    cancellationToken: cancellationToken);
            }
            else
            {
                using MemoryStream ms = new(references.First().Value.ToArray());
                images = (GeneratedImageCollection)await openAIImageClient.GenerateImageEditsAsync(
                    ms,
                    references.First().Key,
                    prompt,
                    variantCount,
                    options: new ImageEditOptions
                    {
                        Size = size,
                    },
                    cancellationToken: cancellationToken);
            }

            return images.Select(i => i.ImageBytes);
        }
    }
}
