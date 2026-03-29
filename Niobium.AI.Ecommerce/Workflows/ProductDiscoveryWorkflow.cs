using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;
using Niobium.AI.Ecommerce.Executors.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class ProductDiscoveryWorkflow(
        UserInputAdaptor<ProductDiscoveryInput> inputAdaptor,
        AdsDiscovererAdaptor adsDiscovererAdaptor,
        AdsDiscoverer adsDiscoverer,
        ProductClusterer productClusterer,
        ProductDiscoveryAggregator aggregator)
        : GenericWorkflow<ProductDiscoveryInput, ProductDiscoveryOutput>
    {
        public override string Id => nameof(ProductDiscoveryWorkflow);

        protected override bool ValidateInput(ProductDiscoveryInput input)
            => !String.IsNullOrWhiteSpace(input.Keyword)
                && Country.TryParse(input.SourceCountry, out _)
                && Country.TryParse(input.TargetCountry, out _);

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding adsDiscovererExecutor = adsDiscoverer.GetBinding(States.RawAds);
            ExecutorBinding productClustererExecutor = productClusterer.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, adsDiscovererAdaptor)
                .AddEdge(adsDiscovererAdaptor, adsDiscovererExecutor)
                .AddEdge(adsDiscovererExecutor, productClustererExecutor)
                .AddEdge(productClustererExecutor, aggregator)
                .WithOutputFrom(aggregator);
            return builder.Build();
        }
    }
}
