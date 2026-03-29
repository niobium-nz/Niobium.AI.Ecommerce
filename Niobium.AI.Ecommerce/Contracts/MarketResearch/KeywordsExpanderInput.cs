namespace Niobium.AI.Ecommerce.Contracts.MarketResearch
{
    public class KeywordsExpanderInput
    {
        public required string CategoryFocus { get; set; }

        public required string Country { get; set; }

        public List<string> SeedKeywords { get; set; } = [];

        public List<KeywordsExpanderOptionalConstraint> OptionalConstraints { get; set; } = [];
    }
}
