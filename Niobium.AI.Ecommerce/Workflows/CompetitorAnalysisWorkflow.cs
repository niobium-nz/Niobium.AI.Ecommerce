using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis;
using Niobium.AI.Ecommerce.Executors.CompetitorAnalysis;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class CompetitorAnalysisWorkflow(
        UserInputAdaptor<CompetitorAnalysisInput> inputAdaptor,
        CompetitionScoutAdaptor competitionScoutAdaptor,
        CompetitionScout competitionScout,
        CompetitorAnalysisAggregator aggregator)
        : GenericWorkflow<CompetitorAnalysisInput, CompetitorAnalysisOutput>
    {
        public override string Id => nameof(CompetitorAnalysisWorkflow);

        protected override bool ValidateInput(CompetitorAnalysisInput input)
            => !String.IsNullOrWhiteSpace(input.Product.Product.LikelyProductName)
                && !String.IsNullOrWhiteSpace(input.Keyword)
                && Country.TryParse(input.SourceCountry, out _)
                && Country.TryParse(input.TargetCountry, out _);

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding competitionScoutExecutor = competitionScout.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, competitionScoutAdaptor)
                .AddEdge(competitionScoutAdaptor, competitionScoutExecutor)
                .AddEdge(competitionScoutExecutor, aggregator)
                .WithOutputFrom(aggregator);
            return builder.Build();
        }
    }
}
