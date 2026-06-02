using Azure.Storage.Blobs;

namespace Niobium.AI.BlobStorage
{
    internal class AzureBlobStorage(BlobServiceClient blobServiceClient) : IFileStorage
    {
        public async Task<Uri> UploadAsync(string name, Stream stream, CancellationToken cancellationToken)
        {
            string endpoint = Environment.GetEnvironmentVariable("AZURE_BLOB_PUBLIC_ENDPOINT")
                ?? throw new Exception("`AZURE_BLOB_PUBLIC_ENDPOINT` must be configured.");
            if (!endpoint.EndsWith('/'))
            {
                endpoint += "/";
            }

            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("$web");
            _ = await container.UploadBlobAsync(name, stream, cancellationToken);
            return new Uri($"{endpoint}{name}");
        }
    }
}
