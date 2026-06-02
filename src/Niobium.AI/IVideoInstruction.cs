namespace Niobium.AI
{
    public interface IVideoInstruction
    {
        string VideoPrompt { get; set; }

        int VideoWidth { get; set; }

        int VideoHeight { get; set; }

        int VideoDurationInSeconds { get; set; }
    }
}
