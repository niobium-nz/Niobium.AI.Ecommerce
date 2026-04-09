namespace Niobium.AI
{
    public interface IVideoClient
    {
        Task<BinaryData> RunAsync(
            string conversationID,
            string prompt,
            int width,
            int height,
            int durationInSeconds,
            CancellationToken cancellationToken);
    }
}
