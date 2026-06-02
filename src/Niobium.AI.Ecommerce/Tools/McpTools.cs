using Microsoft.Extensions.AI;

namespace Niobium.AI.Ecommerce.Tools
{
    public class McpTools(AdsLibraryTool adsLibraryTool) : AI.McpTools
    {
        public IEnumerable<AITool> GetAdsLibraryTools() => [AIFunctionFactory.Create(adsLibraryTool.SearchAds)];
    }
}
