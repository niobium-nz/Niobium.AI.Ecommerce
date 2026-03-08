using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class CompetitionScout(IChatClientFactory clientFactory, AgentTools.McpTools mcpTools, ILogger<CompetitionScout> logger) : TypedGenericLanguageAIAgent<CompetitionScoutInput, CompetitionScoutOutput>(clientFactory, logger)
    {
        public override string Name => nameof(CompetitionScout);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => mcpTools.GetAdsLibraryToolsAsync(cancellationToken);
    }
}
