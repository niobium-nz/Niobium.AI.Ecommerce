using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.AI.Web;

namespace Niobium.AI.Playwright
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IHostApplicationBuilder AddPlaywright(this IHostApplicationBuilder builder)
        {
            builder.AddAI().Services.AddPlaywright(builder.Configuration.GetSection(nameof(PlaywrightOptions)).Bind);
            return builder;
        }

        public static IServiceCollection AddPlaywright(this IServiceCollection services, Action<PlaywrightOptions>? options = null)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            return services.Configure<PlaywrightOptions>(o => options?.Invoke(o))
                .AddSingleton(_ => Microsoft.Playwright.Playwright.CreateAsync().GetAwaiter().GetResult())
                .AddTransient<IWebBrowser, PlaywrightBrowserDriver>();
        }
    }
}
