namespace Niobium.AI.Ecommerce.Contracts
{
    internal class MarketResearchInput
    {
        public required string CategoryFocus { get; set; }

        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public List<string> SeedKeywords { get; set; } = [];

        public List<KeywordsExpanderOptionalConstraint> OptionalConstraints { get; set; } = [];
    }
}
