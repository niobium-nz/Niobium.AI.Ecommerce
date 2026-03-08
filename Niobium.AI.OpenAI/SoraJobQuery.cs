namespace Niobium.AI.OpenAI
{
    internal record SoraJobQuery
    {
        public required string Id { get; init; }

        public required string Object { get; init; }

        public long CreatedAt { get; init; }

        public required string Status { get; init; }

        public long? CompletedAt { get; init; }

        public SoraJobError? Error { get; init; }

        public long? ExpiresAt { get; init; }

        public required string Model { get; init; }

        public int Progress { get; init; }

        public required string Prompt { get; init; }

        public string? RemixedFromVideoId { get; init; }

        public required string Seconds { get; init; }

        public required string Size { get; init; }
    }
}
