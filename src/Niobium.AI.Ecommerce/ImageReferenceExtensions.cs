using System.Net.Http.Headers;

namespace Niobium.AI.Ecommerce
{
    internal static class ImageReferenceExtensions
    {
        private const string DefaultMediaType = "application/octet-stream";

        public static async Task<ImageReference> ToImageReferenceAsync(this string imageSource)
        {
            if (String.IsNullOrWhiteSpace(imageSource))
            {
                throw new ArgumentException("Image source cannot be null or empty.", nameof(imageSource));
            }

            if (Uri.TryCreate(imageSource, UriKind.Absolute, out Uri? absoluteUri))
            {
                if (absoluteUri.IsFile)
                {
                    string localPath = absoluteUri.LocalPath;
                    return new ImageReference
                    {
                        Data = BinaryData.FromFile(localPath, GetMediaTypeFromFileName(localPath)),
                    };
                }

                if (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps)
                {
                    using HttpClient httpClient = new();
                    using HttpResponseMessage response = await httpClient.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    await using Stream stream = await response.Content.ReadAsStreamAsync();
                    return new ImageReference
                    {
                        Data = await BinaryData.FromStreamAsync(stream, GetMediaType(response.Content.Headers))
                    };
                }
            }

            return Path.IsPathFullyQualified(imageSource) || File.Exists(imageSource)
                ? new ImageReference
                {
                    Data = BinaryData.FromFile(imageSource, GetMediaTypeFromFileName(imageSource))
                }
                : throw new NotSupportedException($"Unsupported image source '{imageSource}'. Expected a file path, file URI, or HTTP/HTTPS URL.");
        }

        private static string GetMediaTypeFromFileName(string path)
        {
            string extension = Path.GetExtension(path);
            return String.IsNullOrWhiteSpace(extension)
                ? DefaultMediaType
                : extension.ToLowerInvariant() switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    ".tif" or ".tiff" => "image/tiff",
                    ".svg" => "image/svg+xml",
                    ".ico" => "image/x-icon",
                    ".avif" => "image/avif",
                    _ => DefaultMediaType,
                };
        }

        internal static string GetMediaType(HttpContentHeaders headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            return String.IsNullOrWhiteSpace(headers.ContentType?.MediaType)
                ? DefaultMediaType
                : headers.ContentType.MediaType;
        }
    }
}
