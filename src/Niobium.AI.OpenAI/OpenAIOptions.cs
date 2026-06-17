namespace Niobium.AI.OpenAI
{
    public class OpenAIOptions
    {
        public static readonly string SectionName = nameof(OpenAIOptions).ToUpperInvariant();

        public required string ResponseEndpoint { get; set; }

        public required string ResponseEndpointKey { get; set; }

        public required string ImageEndpoint { get; set; }

        public required string ImageEndpointKey { get; set; }

        public required string VideoEndpoint { get; set; }

        public required string VideoEndpointKey { get; set; }

        public HashSet<string> RetryKeywords { get; set; } = [];

        public int MaxRetries { get; set; } = 3;

        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(3);

        public double RetryBackoffMultiplier { get; set; } = 1d;

        public double RetryJitterFactor { get; set; }

        public TimeSpan? MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    }
}