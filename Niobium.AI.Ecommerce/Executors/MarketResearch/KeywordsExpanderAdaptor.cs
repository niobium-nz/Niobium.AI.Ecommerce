using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.MarketResearch;

namespace Niobium.AI.Ecommerce.Executors.MarketResearch
{
    internal class KeywordsExpanderAdaptor() : Executor<MarketResearchInput, KeywordsExpanderInput>(nameof(KeywordsExpanderAdaptor))
    {
        public override ValueTask<KeywordsExpanderInput> HandleAsync(MarketResearchInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new KeywordsExpanderInput
            {
                CategoryFocus = message.CategoryFocus,
                Country = message.SourceCountry,
                SeedKeywords = message.SeedKeywords,
                OptionalConstraints = message.OptionalConstraints
            });
    }
}
