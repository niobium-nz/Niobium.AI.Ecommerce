using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class KeywordsPlanner(AIProjectClient client, ILogger<KeywordsPlanner> logger) : HostedAIAgent<KeywordsPlannerInput, KeywordsPlannerOutput>(client, logger)
    {
        public override string Name => nameof(KeywordsPlanner);

        protected override IEnumerable<ResponseTool> Tools =>
        [
            ResponseTool.CreateWebSearchPreviewTool(
                WebSearchToolLocation.CreateApproximateLocation(country: "US")
            ),
        ];

        public override Task<KeywordsPlannerOutput> RunAsync(string conversationID, KeywordsPlannerInput input, CancellationToken cancellationToken) 
            => Task.FromResult(new KeywordsPlannerOutput
            {
                CategoryFocus = input.CategoryFocus,
                OptimizedKeywords = [
                    $"{input.CategoryFocus} for {input.Country}",
                    $"Best {input.CategoryFocus} in {input.Country}",
                    $"{input.CategoryFocus} online {input.Country}",
                ]
            });
    }
}
