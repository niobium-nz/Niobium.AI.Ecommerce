namespace Niobium.Ads.Agents
{
    public record CompetitionScoutInput
    {
        public required string Query { get; init; }

        public required string Country { get; init; }

        public string? CategoryName { get; set; }

        public List<string> Notes { get; set; } = [];

        public List<string> AvoidOrExclusionTerms { get; set; } = [];

        public List<ProductInterpretation> ProductInterpretations = [];
    }
}
