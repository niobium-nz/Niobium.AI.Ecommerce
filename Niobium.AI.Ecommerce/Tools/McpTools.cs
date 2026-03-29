using Microsoft.Extensions.AI;

namespace Niobium.AI.Ecommerce.Tools
{
    internal class McpTools(AdsLibraryTool adsLibraryTool) : AI.McpTools
    {
        public Task<IEnumerable<AITool>> GetAdsLibraryToolsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<AITool>>([AIFunctionFactory.Create(adsLibraryTool.SearchAds)]);
    }
}
