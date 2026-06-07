using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Web;

namespace Niobium.AI.Playwright
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddPlaywright(this IServiceCollection services, Action<PlaywrightBrowserLaunchOptions>? options = null)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            services.Configure<PlaywrightBrowserLaunchOptions>(o => options?.Invoke(o));

            return services.AddSingleton(_ => Microsoft.Playwright.Playwright.CreateAsync().GetAwaiter().GetResult())
                .AddTransient<IWebBrowser, PlaywrightBrowserDriver>();
        }
    }
}
