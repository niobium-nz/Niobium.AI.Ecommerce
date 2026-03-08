using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI.BlobStorage
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddBlobStorage(this IServiceCollection services)
        {
            if (!loaded)
            {
                loaded = true;

                _ = services.AddTransient(sp =>
                {
                    var endpoint = Environment.GetEnvironmentVariable("AZURE_BLOB_ENDPOINT")
                        ?? throw new Exception("`AZURE_BLOB_ENDPOINT` must be configured.");
                    var sas = Environment.GetEnvironmentVariable("AZURE_BLOB_SAS")
                        ?? throw new Exception("`AZURE_BLOB_SAS` must be configured.");
                    return new BlobServiceClient(new Uri(endpoint), new AzureSasCredential(sas));
                });
                _ = services.AddTransient<IFileStorage, AzureBlobStorage>();
            }

            return services;
        }
    }
}
