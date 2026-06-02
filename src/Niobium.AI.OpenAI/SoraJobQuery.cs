namespace Niobium.AI.OpenAI
{
    internal record SoraJobQuery
    {
        public required string Id { get; init; }

        public string? Object { get; init; }

        public long CreatedAt { get; init; }

        public required string Status { get; init; }

        public long? CompletedAt { get; init; }

        public SoraJobError? Error { get; init; }

        public long? ExpiresAt { get; init; }

        public string? Model { get; init; }

        public int Progress { get; init; }

        public string? Prompt { get; init; }

        public string? RemixedFromVideoId { get; init; }

        public string? Seconds { get; init; }

        public string? Size { get; init; }
    }
}
