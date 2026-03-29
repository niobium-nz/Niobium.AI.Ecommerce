using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddOpenAI(this IServiceCollection services)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            Niobium.AI.DependencyModule.AddAI(services)
                .AddSingleton<IChatClientFactory, OpenAIChatClientFactory>()
                .AddTransient<IVideoClientFactory, OpenAIVideoClientFactory>()
                .AddHttpClient<SoraVideoClient>(httpClient =>
                {
                    httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("VIDEO_SORA_ENDPOINT")!);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("VIDEO_SORA_KEY")!}");
                }).AddStandardResilienceHandler();
            return services;
        }
    }
}
