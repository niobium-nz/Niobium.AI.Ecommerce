namespace Niobium.AI.Shorts.Contracts
{
    internal class SoraShortProducerInput
    {
        public required string Prompt { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int DurationInSeconds { get; set; }
    }
}
