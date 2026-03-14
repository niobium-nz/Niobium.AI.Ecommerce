using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Shorts.Agents;
using Niobium.AI.Shorts.Workflows;

namespace Niobium.AI.Shorts
{
    public static class DependencyModule
    {
        public static IServiceCollection AddShorts(this IServiceCollection services)
        {
            _ = services.AddAI()
            .AddSingleton<McpTools>()
            .AddTransient<MetaVideoAdCreator>()
            .AddTransient<AttractiveShortProducer>()
            .AddTransient<IWorkflow, AttractiveShortWorkflow>()
            .AddTransient(sp =>
            {
                var endpoint = Environment.GetEnvironmentVariable("AZURE_TABLE_ENDPOINT")
                    ?? throw new Exception("`AZURE_TABLE_ENDPOINT` must be configured.");
                var sas = Environment.GetEnvironmentVariable("AZURE_TABLE_SAS")
                    ?? throw new Exception("`AZURE_TABLE_SAS` must be configured.");
                return new TableServiceClient(new Uri(endpoint), new AzureSasCredential(sas));
            });

            return services;
        }
    }
}
