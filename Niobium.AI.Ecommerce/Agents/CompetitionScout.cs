using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;
using OpenAI;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class CompetitionScout(OpenAIClient client, McpTools mcpTools, ILogger<CompetitionScout> logger) : GenericResponseAIAgent<CompetitionScoutInput, CompetitionScoutOutput>(client, logger)
    {
        public override string Name => nameof(CompetitionScout);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => mcpTools.GetAdsLibraryToolsAsync(cancellationToken);
    }
}
