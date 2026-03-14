namespace Niobium.AI
{
    public interface IResponseWithVideo
    {
        string VideoPrompt { get; set; }

        int VideoWidth { get; set; }

        int VideoHeight { get; set; }

        int VideoDurationInSeconds { get; set; }

        string? VideoUrl { get; set; }
    }
}
