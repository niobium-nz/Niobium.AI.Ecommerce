using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.ProductProfile;

namespace Niobium.AI.Ecommerce.Executors.ProductProfile
{
    internal class ProductProfiler(IChatClientFactory clientFactory, Tools.McpTools tools, ILogger<ProductProfiler> logger) : TypedResponseAgent<ProductProfilerInput, ProductProfilerOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductProfiler);

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override Task OnCleanupAsync(CancellationToken cancellationToken)
            => tools.CleanupPlaywrightTabsAsync(cancellationToken);
    }
}
