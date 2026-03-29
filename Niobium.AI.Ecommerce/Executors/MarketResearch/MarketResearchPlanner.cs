using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.MarketResearch;

namespace Niobium.AI.Ecommerce.Executors.MarketResearch
{
    internal sealed class MarketResearchPlanner() : Executor<KeywordsExpanderOutput, MarketResearchOutput>(nameof(MarketResearchPlanner))
    {
        public override async ValueTask<MarketResearchOutput> HandleAsync(KeywordsExpanderOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            MarketResearchInput userInput = await context.GetUserInput<MarketResearchInput>(cancellationToken);
            return new MarketResearchOutput
            {
                SourceCountry = userInput.SourceCountry,
                TargetCountry = userInput.TargetCountry,
                Keywords = message.OptimizedKeywords,
            };
        }
    }
}
