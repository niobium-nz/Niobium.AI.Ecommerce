using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        public static IServiceCollection AddOpenAI(this IServiceCollection services)
        {
            _ = Niobium.AI.DependencyModule.AddAI(services);
            services
                .AddTransient<IChatClientFactory, OpenAIChatClientFactory>()
                .AddTransient<IVideoClientFactory, OpenAIVideoClientFactory>()
                .AddHttpClient<SoraVideoClient>(httpClient =>
                {
                    httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("VIDEO_SORA_ENDPOINT")!);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("VIDEO_SORA_KEY")!}");
                });
            return services;
        }
    }
}
