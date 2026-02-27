using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class ProductProfiler(AIProjectClient client, ILogger<ProductProfiler> logger) : HostedAIAgent<ProductProfilerInput, ProductProfilerOutput>(client, logger)
    {
        public override string Name => nameof(ProductProfiler);

        protected override ResponseReasoningEffortLevel? Reasoning => ResponseReasoningEffortLevel.High;

        protected override IEnumerable<ResponseTool> Tools =>
        [
            McpTools.PlayWrightMcpTool
        ];
    }
}
