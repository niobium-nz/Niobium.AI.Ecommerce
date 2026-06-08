using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IHostApplicationBuilder AddOpenAI(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenAI(builder.Configuration.GetSection(nameof(OpenAIOptions)).Bind);
            return builder;
        }

        public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<OpenAIOptions> options)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            services.Configure<OpenAIOptions>(options.Invoke);
            services.AddSingleton<OpenAIClientFactory>()
                .AddTransient<IImageClientFactory, OpenAIImageClientFactory>()
                .AddTransient<IChatClientFactory, OpenAIChatClientFactory>()
                .AddTransient<IVideoClientFactory, OpenAIVideoClientFactory>()
                .AddHttpClient<SoraVideoClient>((sp, httpClient) =>
                {
                    IOptions<OpenAIOptions> opt = sp.GetRequiredService<IOptions<OpenAIOptions>>();
                    httpClient.BaseAddress = new Uri(opt.Value.SoraEndpoint.Trim());
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.Value.SoraKey.Trim());
                }).AddStandardResilienceHandler();
            return services;
        }
    }
}
