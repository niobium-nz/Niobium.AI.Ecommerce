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
            .AddTransient<MetaVideoAdPublisher>()
            .AddTransient<AttractiveShortDirector>()
            .AddTransient<IWorkflow, AttractiveShortWorkflow>()
            .AddHttpClient<SoraShortProducer>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")!);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("OPENAI_KEY")!}");
            });

            return services;
        }
    }
}
