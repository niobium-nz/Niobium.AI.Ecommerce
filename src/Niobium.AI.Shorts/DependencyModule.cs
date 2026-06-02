using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Shorts.Executors;

namespace Niobium.AI.Shorts
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddShorts(this IServiceCollection services)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;
            return services.AddAI()
                .AddSingleton<McpTools>()
                .AddTransient<AttractiveShortScreenwriter>()
                .AddTransient<AttractiveShortScreenwriterAdaptor>()
                .AddTransient<MetaVideoAdCreatorAdaptor>()
                .AddTransient<MetaVideoAdCreator>()
                .AddTransient<AttractiveShortProducer>()
                .AddTransient(sp =>
                {
                    string endpoint = Environment.GetEnvironmentVariable("AZURE_TABLE_ENDPOINT")
                        ?? throw new Exception("`AZURE_TABLE_ENDPOINT` must be configured.");
                    string sas = Environment.GetEnvironmentVariable("AZURE_TABLE_SAS")
                        ?? throw new Exception("`AZURE_TABLE_SAS` must be configured.");
                    return new TableServiceClient(new Uri(endpoint), new AzureSasCredential(sas));
                });
        }
    }
}
