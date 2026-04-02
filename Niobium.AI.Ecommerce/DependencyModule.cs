using Microsoft.Extensions.DependencyInjection;
using Niobium.AI.Ecommerce.Executors.CompetitorAnalysis;
using Niobium.AI.Ecommerce.Executors.MarketResearch;
using Niobium.AI.Ecommerce.Executors.ProductDiscovery;
using Niobium.AI.Ecommerce.Executors.ProductNormalization;
using Niobium.AI.Ecommerce.Executors.ProductProfile;
using Niobium.AI.Ecommerce.Executors.ProductVisual;
using Niobium.AI.Ecommerce.Tools;
using Niobium.AI.Ecommerce.Workflows;

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
                .AddSingleton<Tools.McpTools>()
                .AddTransient<IMetaAdsLibrary, TestAdsLibrary>()
                .AddTransient<AdsDiscoverer>()
                .AddTransient<ProductNormalizer>()
                .AddTransient<CompetitionScout>()
                .AddTransient<ProductClusterer>()
                .AddTransient<KeywordsExpander>()
                .AddTransient<ProductProfiler>()
                .AddTransient<CompetitorAnalysisWorkflow>()
                .AddTransient<IWorkflow, ProductCreativityWorkflow>()
                .AddTransient<ProductDiscoveryWorkflow>()
                .AddTransient<ProductProfileWorkflow>()
                .AddTransient<ProductNormalizationWorkflow>()
                .AddTransient<KeywordsExpanderAdaptor>()
                .AddTransient<MarketResearchPlanner>()
                .AddTransient<AdsDiscovererAdaptor>()
                .AddTransient<ProductDiscoveryAggregator>()
                .AddTransient<ProductNormalizerAdaptor>()
                .AddTransient<ProductNormalizationAggregator>()
                .AddTransient<CompetitionScoutAdaptor>()
                .AddTransient<CompetitorAnalysisAggregator>()
                .AddTransient<ProductProfilerAdaptor>()
                .AddTransient<ProductVisualBuilderAdaptor>()
                .AddTransient<ProductVisualBuilder>();
        }
    }
}
