using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Shorts.Agents;
using Niobium.AI.Shorts.Workflows;

namespace Niobium.AI.Shorts
{
    public static class DependencyModule
    {
        public static IServiceCollection AddShorts(this IServiceCollection services)
        {
            services.AddAI()
            .AddSingleton<McpTools>()
            .AddTransient<MetaVideoAdCreator>()
            .AddTransient<AttractiveShortProducer>()
            .AddTransient<IWorkflow, AttractiveShortWorkflow>();

            return services;
        }
    }
}
