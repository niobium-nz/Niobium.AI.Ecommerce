using Microsoft.Extensions.AI;

namespace Niobium.AI.Ecommerce.AgentTools
{
    internal class McpTools(AdsLibraryTool adsLibraryTool) : Niobium.AI.McpTools
    {
        public Task<IEnumerable<AITool>> GetAdsLibraryToolsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<AITool>>([AIFunctionFactory.Create(adsLibraryTool.SearchAds)]);
    }
}
