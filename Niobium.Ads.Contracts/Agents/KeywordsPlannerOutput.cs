namespace Niobium.Ads.Agents
{
    public class KeywordsPlannerOutput
    {
        public required string CategoryFocus { get; set; }

        public List<string> OptimizedKeywords { get; set; } = [];
    }
}
