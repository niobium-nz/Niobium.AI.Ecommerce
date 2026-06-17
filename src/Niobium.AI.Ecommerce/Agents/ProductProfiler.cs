using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Web;

namespace Niobium.AI.Ecommerce.Agents
{
    public class ProductProfiler(IChatClientFactory clientFactory, IWebBrowser browser, ILogger<ProductProfiler> logger)
        : TypedResponseAgent<ProductProfilerInput, ProductProfilerOutput>(clientFactory, logger)
    {
        public override string Id => nameof(ProductProfiler);

        protected override string? ModelProvider => "ApiMart";

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override IEnumerable<AITool> GetTools() => browser.AsAITools();
    }
}
