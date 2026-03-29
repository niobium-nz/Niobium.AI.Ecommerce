using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.MarketResearch;
using Niobium.AI.Ecommerce.Executors.MarketResearch;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class MarketResearchWorkflow(
        UserInputAdaptor<MarketResearchInput> inputAdaptor,
        KeywordsExpanderAdaptor keywordsExpanderAdaptor,
        KeywordsExpander keywordsExpander,
        MarketResearchPlanner marketResearchPlanner)
        : GenericWorkflow<MarketResearchInput, MarketResearchOutput>
    {
        public override string Id => nameof(MarketResearchWorkflow);

        protected override bool ValidateInput(MarketResearchInput input)
            => !String.IsNullOrWhiteSpace(input.CategoryFocus)
                && Country.TryParse(input.SourceCountry, out _)
                && Country.TryParse(input.TargetCountry, out _);

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding keywordsExpanderExecutor = keywordsExpander.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, keywordsExpanderAdaptor)
                .AddEdge(keywordsExpanderAdaptor, keywordsExpanderExecutor)
                .AddEdge(keywordsExpanderExecutor, marketResearchPlanner)
                .WithOutputFrom(marketResearchPlanner);
            return builder.Build();
        }
    }
}
