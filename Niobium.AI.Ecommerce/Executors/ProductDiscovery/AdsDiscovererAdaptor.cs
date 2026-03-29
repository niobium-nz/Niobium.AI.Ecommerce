using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Executors.ProductDiscovery
{
    internal class AdsDiscovererAdaptor() : Executor<ProductDiscoveryInput, AdsDiscovererInput>(nameof(AdsDiscovererAdaptor))
    {
        public override ValueTask<AdsDiscovererInput> HandleAsync(ProductDiscoveryInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new AdsDiscovererInput
            {
                Keyword = message.Keyword,
                Country = message.TargetCountry
            });
    }
}
