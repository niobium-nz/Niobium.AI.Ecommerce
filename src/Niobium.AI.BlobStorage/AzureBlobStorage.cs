using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Niobium.AI.BlobStorage
{
    internal class AzureBlobStorage(BlobServiceClient blobServiceClient, IOptions<BlobOptions> options) : IFileStorage
    {
        public async Task<Uri> UploadAsync(string name, Stream stream, CancellationToken cancellationToken)
        {
            string endpoint = options.Value.DataEndpoint;
            if (!endpoint.EndsWith('/'))
            {
                endpoint += "/";
            }

            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
            await container.UploadBlobAsync(name, stream, cancellationToken);
            return new Uri($"{endpoint}{name}");
        }
    }
}
