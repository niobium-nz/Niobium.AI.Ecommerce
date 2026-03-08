namespace Niobium.AI
{
    public interface IResponseWithVideo
    {
        string VideoPrompt { get; }

        int VideoWidth { get; }

        int VideoHeight { get; }

        int VideoDurationInSeconds { get; }

        Uri? VideoUrl { get; set; }
    }
}
