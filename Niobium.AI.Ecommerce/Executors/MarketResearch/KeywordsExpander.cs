using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.MarketResearch;

namespace Niobium.AI.Ecommerce.Executors.MarketResearch
{
    internal class KeywordsExpander(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<KeywordsExpander> logger) : TypedResponseAgent<KeywordsExpanderInput, KeywordsExpanderOutput>(clientFactory, logger)
    {
        public override string Id => nameof(KeywordsExpander);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken)
            => tools.GetWebSearchTools(cancellationToken);

        protected override async Task OnResponseGotAsync(string conversationID, KeywordsExpanderInput input, KeywordsExpanderOutput? output, CancellationToken cancellationToken)
        {
            // Remove duplicates from the output keywords list, if any
            output?.OptimizedKeywords = [.. output.OptimizedKeywords.Distinct()];

            await base.OnResponseGotAsync(conversationID, input, output, cancellationToken);
        }
    }
}
