using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddEcommerce(this IServiceCollection services)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            return services.AddAI()
                .AddTransient<AdsLibraryTool>()
                .AddSingleton<Tools.ToolBox>()
                .AddTransient<IMetaAdsLibrary, TestAdsLibrary>();
        }
    }
}
