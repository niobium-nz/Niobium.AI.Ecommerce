using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.ProductVisual;

namespace Niobium.AI.Ecommerce.Executors.ProductVisual
{
    internal class ProductVisualBuilderAdaptor() : Executor<ProductCreativityInput, ProductVisualBuilderInput>(nameof(ProductVisualBuilderAdaptor))
    {
        public override ValueTask<ProductVisualBuilderInput> HandleAsync(ProductCreativityInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            _ = new FileInfo(message.Photos);
            return ValueTask.FromResult(new ProductVisualBuilderInput
            {
                Form = ImageForm.Squared,
                References =
                [
                    new ImageReference
                    {
                        Data = BinaryData.FromFile(message.Photos),
                        MediaType = "image/png",
                    }
                ]
            });
        }
    }
}
