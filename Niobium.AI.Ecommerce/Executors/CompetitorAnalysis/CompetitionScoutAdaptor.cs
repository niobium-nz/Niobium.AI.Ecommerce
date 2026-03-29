using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis;

namespace Niobium.AI.Ecommerce.Executors.CompetitorAnalysis
{
    internal class CompetitionScoutAdaptor() : Executor<CompetitorAnalysisInput, CompetitionScoutInput>(nameof(CompetitionScoutAdaptor))
    {
        public override ValueTask<CompetitionScoutInput> HandleAsync(CompetitorAnalysisInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new CompetitionScoutInput
            {
                Query = message.NormalizedKeyword,
                Country = message.TargetCountry,
                CategoryName = message.Product.Product.CategoryGuess,
                Notes = message.CompetitorAnalysisNotes,
                AvoidOrExclusionTerms = message.AvoidOrExclusionTerms,
                ProductInterpretations = message.ProductInterpretations
            });
    }
}
