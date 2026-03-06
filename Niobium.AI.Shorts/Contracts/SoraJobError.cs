namespace Niobium.AI.Shorts.Contracts
{
    internal record SoraJobError
    {
        public string? Code { get; init; }

        public string? Message { get; init; }
    }
}
