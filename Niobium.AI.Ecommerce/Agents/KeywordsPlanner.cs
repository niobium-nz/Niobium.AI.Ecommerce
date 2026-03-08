using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class KeywordsPlanner(IChatClientFactory clientFactory, McpTools tools, ILogger<KeywordsPlanner> logger) : TypedGenericLanguageAIAgent<KeywordsPlannerInput, KeywordsPlannerOutput>(clientFactory, logger)
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
