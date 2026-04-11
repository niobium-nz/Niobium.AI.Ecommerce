using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.MarketStrategy;
using Niobium.AI.Ecommerce.Executors.MarketStrategy;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class MarketingWorkflow(
        UserInputAdaptor<MarketStrategyInput> inputAdaptor,
        MarketStrategist marketStrategist,
        ILogger<MarketingWorkflow> logger)
        : GenericWorkflow<MarketStrategyInput, MarketStrategyOutput>
    {
        public override string Id => nameof(MarketingWorkflow);

        protected override bool ValidateInput(MarketStrategyInput input)
        {
            if (String.IsNullOrWhiteSpace(input.CompetitorUsedProductName) || input.CompetitorClaims.Count <= 0)
            {
                logger.LogInformation("Competitor product info is missing. Skipping profiling for this product.");
                return false;
            }

            if (String.IsNullOrWhiteSpace(input.COGSPerUnit))
            {
                logger.LogInformation("COGS per unit is missing for product {productName}. Skipping profiling for this product.",
                    input.CompetitorUsedProductName);
                return false;
            }

            if (String.IsNullOrWhiteSpace(input.TargetMarketCountry))
            {
                logger.LogInformation("Target market country is missing for product {productName}. Skipping profiling for this product.",
                    input.CompetitorUsedProductName);
                return false;
            }

            return true;
        }

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding marketStrategistExecutor = marketStrategist.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, marketStrategistExecutor)
                .WithOutputFrom(marketStrategistExecutor);
            return builder.Build();
        }
    }
}
