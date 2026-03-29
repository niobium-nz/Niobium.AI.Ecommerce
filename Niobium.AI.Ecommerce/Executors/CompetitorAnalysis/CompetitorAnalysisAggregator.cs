using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis;

namespace Niobium.AI.Ecommerce.Executors.CompetitorAnalysis
{
    internal class CompetitorAnalysisAggregator(ILogger<CompetitorAnalysisAggregator> logger) : Executor<CompetitionScoutOutput, CompetitorAnalysisOutput>(nameof(CompetitorAnalysisAggregator))
    {
        public override async ValueTask<CompetitorAnalysisOutput> HandleAsync(CompetitionScoutOutput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CompetitorAnalysisInput userInput = await context.GetUserInput<CompetitorAnalysisInput>(cancellationToken);

            if (!String.IsNullOrWhiteSpace(message.RawAdsDiscovered.McpError))
            {
                logger.LogError("MCP error {mcpError} found for competitive search query {competitiveSearchQuery} for product {productName}. Skipping this competitive search query.",
                    message.RawAdsDiscovered.McpError,
                    message.Query,
                    userInput.Product.Product.LikelyProductName);
            }

            return new CompetitorAnalysisOutput
            {
                SourceCountry = userInput.SourceCountry,
                TargetCountry = userInput.TargetCountry,
                Keyword = userInput.Keyword,
                Product = userInput.Product,
                CompetitionSignal = message.CompetitionSignal,
            };
        }
    }
}
