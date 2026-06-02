namespace Niobium.AI.Shorts.Contracts
{
    internal class SubtitlePlan
    {
        public required SubtitleStyle Style { get; set; }

        public List<SubtitleCaption> Captions { get; set; } = [];
    }
}
