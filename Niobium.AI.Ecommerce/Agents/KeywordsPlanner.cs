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

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);

        public override Task<KeywordsPlannerOutput> RunAsync(string conversationID, KeywordsPlannerInput input, CancellationToken cancellationToken)
            => Task.FromResult(new KeywordsPlannerOutput
            {
                CategoryFocus = input.CategoryFocus,
                OptimizedKeywords = [
                    $"{input.CategoryFocus} for {input.Country}",
                    $"Best {input.CategoryFocus} in {input.Country}",
                    $"{input.CategoryFocus} online {input.Country}",
                ]
            });
    }
}
