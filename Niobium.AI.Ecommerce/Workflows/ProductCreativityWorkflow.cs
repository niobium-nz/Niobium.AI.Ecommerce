using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.ProductVisual;
using Niobium.AI.Ecommerce.Executors.ProductVisual;

namespace Niobium.AI.Ecommerce.Workflows
{
    internal class ProductCreativityWorkflow(
        UserInputAdaptor<ProductCreativityInput> userInputAdaptor,
        ProductVisualBuilderAdaptor productVisualBuilderAdaptor,
        ProductVisualBuilder productVisualBuilder) : GenericWorkflow<ProductCreativityInput, ProductCreativityOutput>
    {
        public override string Id => nameof(ProductCreativityWorkflow);

        protected override Workflow BuildWorkflow()
        {
            ExecutorBinding productVisualBuilderExecutor = productVisualBuilder.GetBinding();

            WorkflowBuilder builder = new WorkflowBuilder(userInputAdaptor)
                .AddEdge(userInputAdaptor, productVisualBuilderAdaptor)
                .AddEdge(productVisualBuilderAdaptor, productVisualBuilderExecutor)
                .WithOutputFrom(productVisualBuilderExecutor);
            return builder.Build();
        }
    }
}
