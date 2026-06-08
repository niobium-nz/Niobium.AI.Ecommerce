using Microsoft.DurableTask;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class ExpandableProductDiscoveryFlow : TaskOrchestrator<MarketResearchInput, IEnumerable<ProductDiscoveryOutput>>
    {
        public override async Task<IEnumerable<ProductDiscoveryOutput>> RunAsync(TaskOrchestrationContext context, MarketResearchInput input)
        {
            if (String.IsNullOrWhiteSpace(input.CategoryFocus)
                || !Country.TryParse(input.SourceCountry, out _)
                || !Country.TryParse(input.TargetCountry, out _))
            {
                throw new ArgumentException($"Invalid input parameters. CategoryFocus: {input.CategoryFocus}, SourceCountry: {input.SourceCountry}, TargetCountry: {input.TargetCountry}");
            }

            IResponseGenerator<KeywordsExpanderInput, KeywordsExpanderOutput> keywordsExpander = context.GetAgent<KeywordsExpander, KeywordsExpanderInput, KeywordsExpanderOutput>();
            KeywordsExpanderOutput researchResult = await keywordsExpander.RunAsync(new KeywordsExpanderInput
            {
                CategoryFocus = input.CategoryFocus,
                Country = input.SourceCountry,
                SeedKeywords = input.SeedKeywords,
                OptionalConstraints = input.OptionalConstraints
            });

            IEnumerable<string> keywords = researchResult.OptimizedKeywords.Where(k => !String.IsNullOrWhiteSpace(k)).Distinct();
            IEnumerable<Task<IEnumerable<ProductDiscoveryOutput>>> subOrchestrations = keywords.Select(keyword =>
                context.CallSubOrchestratorAsync<IEnumerable<ProductDiscoveryOutput>>(nameof(ProductDiscoveryFlow), new ProductDiscoveryInput
                {
                    JobId = Guid.NewGuid(),
                    Keyword = keyword,
                    SourceCountry = input.SourceCountry,
                    TargetCountry = input.TargetCountry,
                }));

            IEnumerable<ProductDiscoveryOutput>[] results = await Task.WhenAll(subOrchestrations);
            return results.SelectMany(r => r);
        }
    }
}
