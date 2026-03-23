using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class KeywordsPlanner(IChatClientFactory clientFactory, McpTools tools, ILogger<KeywordsPlanner> logger) : TypedResponseAgent<KeywordsPlannerInput, KeywordsPlannerOutput>(clientFactory, logger)
    {
        public override string Id => nameof(KeywordsPlanner);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override async Task OnResponseGotAsync(string conversationID, KeywordsPlannerInput input, KeywordsPlannerOutput? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnResponseGotAsync(conversationID, input, output, cancellationToken);
        }
    }
}
