namespace Niobium.AI.Ecommerce.Contracts.MarketResearch
{
    internal class MarketResearchOutput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public List<string> Keywords { get; set; } = [];
    }
}
