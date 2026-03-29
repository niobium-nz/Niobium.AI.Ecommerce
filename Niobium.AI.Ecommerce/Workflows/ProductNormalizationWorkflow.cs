using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Contracts.ProductNormalization;
using Niobium.AI.Ecommerce.Executors.ProductNormalization;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class ProductNormalizationWorkflow(
        UserInputAdaptor<ProductNormalizationInput> inputAdaptor,
        ProductNormalizerAdaptor productNormalizerAdaptor,
        ProductNormalizer productNormalizer,
        ProductNormalizationAggregator aggregator)
        : GenericWorkflow<ProductNormalizationInput, ProductNormalizationOutput>
    {
        public override string Id => nameof(ProductNormalizationWorkflow);

        protected override bool ValidateInput(ProductNormalizationInput input)
            => !String.IsNullOrWhiteSpace(input.Product.Product.LikelyProductName)
                && !String.IsNullOrWhiteSpace(input.Keyword)
                && Country.TryParse(input.SourceCountry, out _)
                && Country.TryParse(input.TargetCountry, out _);

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding productNormalizerExecutor = productNormalizer.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, productNormalizerAdaptor)
                .AddEdge(productNormalizerAdaptor, productNormalizerExecutor)
                .AddEdge(productNormalizerExecutor, aggregator)
                .WithOutputFrom(aggregator);
            return builder.Build();
        }
    }
}
