namespace Niobium.AI.OpenAI
{
    public class OpenAIOptions
    {
        public required string LLMEndpoint { get; set; }

        public required string LLMKey { get; set; }

        public required string SoraEndpoint { get; set; }

        public required string SoraKey { get; set; }

        public HashSet<string> RetryKeywords { get; set; } = [];

        public int MaxRetries { get; set; }

        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(3);

        public double RetryBackoffMultiplier { get; set; } = 1d;

        public double RetryJitterFactor { get; set; }

        public TimeSpan? MaxRetryDelay { get; set; }
    }
}