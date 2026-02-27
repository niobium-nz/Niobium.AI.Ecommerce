using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class ProductNormalizer(AIProjectClient client, ILogger<ProductNormalizer> logger) : HostedAIAgent<ProductNormalizerInput, ProductNormalizerOutput>(client, logger)
    {
        public override string Name => nameof(ProductNormalizer);

        protected override ResponseReasoningEffortLevel? Reasoning => ResponseReasoningEffortLevel.Medium;

        protected override IEnumerable<ResponseTool> Tools =>
        [
            ResponseTool.CreateWebSearchPreviewTool(
                WebSearchToolLocation.CreateApproximateLocation(country: "AU")
            ),
        ];
    }
}
