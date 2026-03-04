namespace Niobium.Ads.Agents
{
    public class KeywordsPlannerInput
    {
        public required string CategoryFocus { get; set; }

        public required string Country { get; set; }

        public List<string> SeedKeywords { get; set; } = [];

        public List<KeywordsPlannerOptionalConstraint> OptionalConstraints { get; set; } = [];
    }
}
