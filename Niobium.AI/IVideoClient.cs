namespace Niobium.AI
{
    public interface IVideoClient
    {
        Task<FileInfo> RunAsync(
            string conversationID,
            string prompt,
            int width,
            int height,
            int durationInSeconds,
            CancellationToken cancellationToken);
    }
}
