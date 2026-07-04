namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ReviewSimulatorOutput : List<ReviewSimulation>
    {
    }

    internal class ReviewSimulation
    {
        public required string ReviewText { get; set; }
        public required int Rating { get; init; }
        public string? VideoPrompt { get; init; }
    }
}
