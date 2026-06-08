using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Niobium.AI.BlobStorage
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IHostApplicationBuilder AddBlobStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddBlobStorage(builder.Configuration.GetSection(nameof(BlobOptions)).Bind);
            return builder;
        }

        public static IServiceCollection AddBlobStorage(this IServiceCollection services, Action<BlobOptions> options)
        {
            if (!loaded)
            {
                loaded = true;

                services.Configure<BlobOptions>(options.Invoke)
                .AddTransient<IFileStorage, AzureBlobStorage>()
                .AddTransient(sp =>
                {
                    IOptions<BlobOptions> options = sp.GetRequiredService<IOptions<BlobOptions>>();
                    return new BlobServiceClient(new Uri(options.Value.ControlEndpoint), new AzureSasCredential(options.Value.AccessToken));
                });
            }

            return services;
        }
    }
}
