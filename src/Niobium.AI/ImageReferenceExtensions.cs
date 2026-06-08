using System.Net.Http.Headers;
using System.Net.Mime;

namespace Niobium.AI
{
    public static class ImageReferenceExtensions
    {
        private const string DefaultMediaType = "application/octet-stream";

        public static async Task<Uri> ToTempFileAsync(this BinaryData data, CancellationToken cancellationToken)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), GetImageFileExtension(data.MediaType)));
            await File.WriteAllBytesAsync(tempFilePath, data.ToArray(), cancellationToken);
            return new Uri(tempFilePath);
        }

        public static async Task<ImageReference> ToImageReferenceAsync(this Uri imageSource)
            => await imageSource.AbsolutePath.ToImageReferenceAsync();

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
                        Name = GetFileName(absoluteUri),
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
                        Data = await BinaryData.FromStreamAsync(stream, GetMediaType(response.Content.Headers)),
                        Name = GetFileName(absoluteUri),
                    };
                }
            }

            return Path.IsPathFullyQualified(imageSource) || File.Exists(imageSource)
                ? new ImageReference
                {
                    Data = BinaryData.FromFile(imageSource, GetMediaTypeFromFileName(imageSource)),
                    Name = Path.GetFileName(imageSource),
                }
                : throw new NotSupportedException($"Unsupported image source '{imageSource}'. Expected a file path, file URI, or HTTP/HTTPS URL.");
        }

        private static string GetImageFileExtension(string? mediaType)
            => mediaType switch
            {
                MediaTypeNames.Image.Gif => ".gif",
                MediaTypeNames.Image.Jpeg => ".jpg",
                MediaTypeNames.Image.Tiff => ".tiff",
                "image/png" => ".png",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                "image/x-icon" => ".ico",
                "image/avif" => ".avif",
                _ => ".bin",
            };

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

        private static string GetFileName(Uri uri) => Path.GetFileName(uri.LocalPath);

        internal static string GetMediaType(HttpContentHeaders headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            return String.IsNullOrWhiteSpace(headers.ContentType?.MediaType)
                ? DefaultMediaType
                : headers.ContentType.MediaType;
        }
    }
}
