namespace Niobium.AI.Shorts.Contracts
{
    internal class AttractiveShortWorkflowOutput
    {
        public required string Status { get; init; }

        public string? AdName { get; init; }

        public List<string> ScreenshotFullFilePath { get; init; } = [];
    }
}
