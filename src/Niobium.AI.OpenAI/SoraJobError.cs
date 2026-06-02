namespace Niobium.AI.OpenAI
{
    internal record SoraJobError
    {
        public string? Code { get; init; }

        public string? Message { get; init; }
    }
}
