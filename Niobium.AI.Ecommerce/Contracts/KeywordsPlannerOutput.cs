namespace Niobium.AI.Ecommerce.Contracts
{
    public class KeywordsPlannerOutput
    {
        public required string CategoryFocus { get; set; }

        public List<string> OptimizedKeywords { get; set; } = [];
    }
}
