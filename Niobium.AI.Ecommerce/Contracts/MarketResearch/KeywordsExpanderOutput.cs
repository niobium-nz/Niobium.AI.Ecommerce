namespace Niobium.AI.Ecommerce.Contracts.MarketResearch
{
    public class KeywordsExpanderOutput
    {
        public required string CategoryFocus { get; set; }

        public List<string> OptimizedKeywords { get; set; } = [];
    }
}
