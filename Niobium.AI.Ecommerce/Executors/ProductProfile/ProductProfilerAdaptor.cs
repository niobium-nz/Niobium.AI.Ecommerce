using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.ProductProfile;

namespace Niobium.AI.Ecommerce.Executors.ProductProfile
{
    internal class ProductProfilerAdaptor() : Executor<ProductProfileInput, ProductProfilerInput>(nameof(ProductProfilerAdaptor))
    {
        public override ValueTask<ProductProfilerInput> HandleAsync(ProductProfileInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new ProductProfilerInput
            {
                LandingPageUrl = message.Ad.Snapshot!.LinkUrl!
            });
    }
}
