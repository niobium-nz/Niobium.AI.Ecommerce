namespace Niobium.AI.Shorts.Contracts
{
    internal class SubtitleStyle
    {
        public int FontSizePt { get; set; }

        public int OutlineWidthPt { get; set; }

        public required string ColorRgb { get; set; }

        public required string OutlineRgb { get; set; }

        public required string SafeArea { get; set; }
    }
}
