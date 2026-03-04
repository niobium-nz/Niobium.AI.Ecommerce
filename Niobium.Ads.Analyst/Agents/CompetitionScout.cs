using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.Ads.Agents;
using OpenAI;

namespace Niobium.Ads.Analyst.Agents
{
    internal class CompetitionScout(OpenAIClient client, McpTools mcpTools, ILogger<CompetitionScout> logger) : GenericAIAgent<CompetitionScoutInput, CompetitionScoutOutput>(client, logger)
    {
        public override string Name => nameof(CompetitionScout);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => mcpTools.GetAdsLibraryToolsAsync(cancellationToken);
    }
}
