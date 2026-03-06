namespace Niobium.AI.Shorts.Contracts
{
    internal class SubtitleCaption
    {
        public double Start { get; set; }

        public double End { get; set; }

        public List<string> Text { get; set; } = [];

        public string? Emphasis { get; set; }
    }
}
