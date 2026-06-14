using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IHostApplicationBuilder AddEcommerce(this IHostApplicationBuilder builder)
        {
            builder.AddAI().Services.AddEcommerce(builder.Configuration.GetSection(nameof(EcommerceOptions)).Bind);
            return builder;
        }

        public static IServiceCollection AddEcommerce(this IServiceCollection services, Action<EcommerceOptions> options)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            return services.Configure<EcommerceOptions>(options.Invoke)
                .AddTransient<AdsLibraryTool>()
                .AddSingleton<Tools.ToolBox>()
                .AddTransient<IMetaAdsLibrary, ScrapecreatorsMetaAdsLibrary>();
        }
    }
}
