using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis;

namespace Niobium.AI.Ecommerce.Executors.CompetitorAnalysis
{
    internal class CompetitionScout(IChatClientFactory clientFactory, Tools.McpTools mcpTools, ILogger<CompetitionScout> logger) : TypedResponseAgent<CompetitionScoutInput, CompetitionScoutOutput>(clientFactory, logger)
    {
        public override string Id => nameof(CompetitionScout);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => mcpTools.GetAdsLibraryToolsAsync(cancellationToken);
    }
}
