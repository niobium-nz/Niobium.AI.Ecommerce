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
            builder.AddAI().Services.AddOpenAI(builder.Configuration.GetSection(OpenAIOptions.SectionName).Bind);
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
                .AddHttpClient<SoraVideoClient>().AddStandardResilienceHandler();
            return services;
        }
    }
}
