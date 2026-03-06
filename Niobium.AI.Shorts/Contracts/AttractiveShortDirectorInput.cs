namespace Niobium.AI.Shorts.Contracts
{
    internal class AttractiveShortDirectorInput
    {
        public required string BusinessName { get; set; }

        public required string Location { get; set; }

        public required string BusinessType { get; set; }

        public List<string> ProductsSold { get; set; } = [];

        public string? TypicalSpend { get; set; }
    }
}
