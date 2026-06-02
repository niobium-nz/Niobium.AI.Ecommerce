namespace Niobium.AI.Shorts.Contracts
{
    /// <summary>
    /// Result contract returned after attempting to publish a Meta video ad.
    /// Serialized JSON must conform to the consumer-facing schema with snake_case names.
    /// </summary>
    internal record MetaVideoAdCreatorOutput
    {
        public required string Status { get; init; }

        public string? AdName { get; init; }

        public string? CampaignMatched { get; init; }

        public string? AdSetMatched { get; init; }

        public List<string> Warnings { get; init; } = [];

        public string? ExactFailureStep { get; init; }

        public List<string> ScreenshotFullFilePath { get; init; } = [];
    }
}
