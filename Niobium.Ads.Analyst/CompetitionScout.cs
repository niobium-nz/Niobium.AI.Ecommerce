using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class CompetitionScout(AIProjectClient client, ILogger<CompetitionScout> logger) : HostedAIAgent<CompetitionScoutInput, string>(client, logger)
    {
        public override string Name => nameof(CompetitionScout);

        protected override ResponseReasoningEffortLevel? Reasoning => ResponseReasoningEffortLevel.Medium;

        protected override IEnumerable<ResponseTool> Tools =>
        [
            McpTools.AdsLibraryMcpTool
        ];
    }
}
