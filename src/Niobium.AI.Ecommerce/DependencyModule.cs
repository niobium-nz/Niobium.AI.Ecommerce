using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

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
                .AddTransient<IMetaAdsLibrary, TestAdsLibrary>();
        }
    }
}
