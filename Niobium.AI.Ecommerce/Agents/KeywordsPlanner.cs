using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;
using OpenAI;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class KeywordsPlanner(OpenAIClient client, McpTools tools, ILogger<KeywordsPlanner> logger) : GenericResponseAIAgent<KeywordsPlannerInput, KeywordsPlannerOutput>(client, logger)
    {
        public override string Name => nameof(KeywordsPlanner);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override async Task OnRanAsync(string conversationID, KeywordsPlannerInput input, KeywordsPlannerOutput? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnRanAsync(conversationID, input, output, cancellationToken);
        }
    }
}
