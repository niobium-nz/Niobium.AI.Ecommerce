namespace Niobium.AI
{
    public interface IVideoClient
    {
        Task<BinaryData> RunAsync(
            string prompt,
            int width,
            int height,
            int durationInSeconds,
            CancellationToken cancellationToken);
    }
}
