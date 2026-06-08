using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<OpenAIClientOptions> options)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            services.Configure<OpenAIClientOptions>(options.Invoke);
            services.AddSingleton<OpenAIClientFactory>()
                .AddTransient<IImageClientFactory, OpenAIImageClientFactory>()
                .AddTransient<IChatClientFactory, OpenAIChatClientFactory>()
                .AddTransient<IVideoClientFactory, OpenAIVideoClientFactory>()
                .AddHttpClient<SoraVideoClient>((sp, httpClient) =>
                {
                    IOptions<OpenAIClientOptions> opt = sp.GetRequiredService<IOptions<OpenAIClientOptions>>();
                    httpClient.BaseAddress = new Uri(opt.Value.SoraEndpoint.Trim());
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.Value.SoraKey.Trim());
                }).AddStandardResilienceHandler();
            return services;
        }
    }
}
