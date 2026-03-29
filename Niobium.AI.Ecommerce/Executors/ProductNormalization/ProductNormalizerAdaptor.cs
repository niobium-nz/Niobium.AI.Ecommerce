using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.ProductNormalization;

namespace Niobium.AI.Ecommerce.Executors.ProductNormalization
{
    internal class ProductNormalizerAdaptor() : Executor<ProductNormalizationInput, ProductNormalizerInput>(nameof(ProductNormalizerAdaptor))
    {
        public override async ValueTask<ProductNormalizerInput> HandleAsync(ProductNormalizationInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
            => new()
            {
                ProductName = message.Product.Product.LikelyProductName!,
                CategoryName = message.Product.Product.CategoryGuess,
                KnownFeatures = message.Product.Product.KnownFeatures,
                Country = message.SourceCountry,
            };
    }
}
