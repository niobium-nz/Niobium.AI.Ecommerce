namespace Niobium.AI.Shorts.Contracts
{
    internal class AttractiveShortScreenwriterOutput : IVideoInstruction
    {
        public required string VideoIdea { get; set; }

        public required string VideoPrompt { get; set; }

        public int VideoWidth { get; set; }

        public int VideoHeight { get; set; }

        public int VideoDurationInSeconds { get; set; }

        public required string SocialPost { get; set; }

        public List<string> SocialPostTags { get; set; } = [];

        public required SubtitlePlan SubtitlePlan { get; set; }
    }
}
