using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Workflows;

namespace Niobium.AI.Ecommerce
{
    public static class DependencyModule
    {
        public static IServiceCollection AddEcommerce(this IServiceCollection services) => services.AddAI()
            .AddTransient<AdsLibraryTool>()
            .AddSingleton<McpTools>()
            .AddTransient<IWorkflow, EcommerceAnalystWorkflow>()
            .AddTransient<IMetaAdsLibrary, TestAdsLibrary>()
            .AddTransient<AdsDiscoverer>()
            .AddTransient<ProductNormalizer>()
            .AddTransient<CompetitionScout>()
            .AddTransient<ProductClusterer>()
            .AddTransient<KeywordsPlanner>()
            .AddTransient<ProductProfiler>()
            .AddTransient<EcommerceAnalystWorkflow>();
    }
}
