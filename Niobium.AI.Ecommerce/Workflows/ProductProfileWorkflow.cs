using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.ProductProfile;
using Niobium.AI.Ecommerce.Executors.ProductProfile;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class ProductProfileWorkflow(
        UserInputAdaptor<ProductProfileInput> inputAdaptor,
        ProductProfilerAdaptor productProfilerAdaptor,
        ProductProfiler productProfiler,
        ILogger<ProductProfileWorkflow> logger)
        : GenericWorkflow<ProductProfileInput, ProductProfilerOutput>
    {
        public override string Id => nameof(ProductProfileWorkflow);

        protected override bool ValidateInput(ProductProfileInput input)
        {
            if (input.Ad.Snapshot == null || String.IsNullOrWhiteSpace(input.Ad.Snapshot.LinkUrl))
            {
                logger.LogInformation("Ad snapshot or LinkUrl is missing for product {productName}. Skipping profiling for this product.",
                    input.Product.LikelyProductName);
                return false;
            }

            return true;
        }

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding productProfilerExecutor = productProfiler.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(inputAdaptor)
                .AddEdge(inputAdaptor, productProfilerAdaptor)
                .AddEdge(productProfilerAdaptor, productProfilerExecutor)
                .WithOutputFrom(productProfilerExecutor);
            return builder.Build();
        }
    }
}
