using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Niobium.AI.WebScraper.Firecrawl
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IHostApplicationBuilder AddFirecrawl(this IHostApplicationBuilder builder)
        {
            builder.AddAI().Services.AddFirecrawl(builder.Configuration.GetSection(FirecrawlOptions.SectionName).Bind);
            return builder;
        }

        public static IServiceCollection AddFirecrawl(this IServiceCollection services, Action<FirecrawlOptions>? options = null)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            services.Configure<FirecrawlOptions>(o => options?.Invoke(o))
                .AddHttpClient<IWebScraper, FirecrawlWebScraper>().AddStandardResilienceHandler();

            return services;
        }
    }
}
