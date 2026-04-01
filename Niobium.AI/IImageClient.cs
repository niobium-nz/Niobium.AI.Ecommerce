namespace Niobium.AI
{
    public interface IImageClient
    {
        Task<IEnumerable<Uri>> RunAsync(
            string conversationID,
            string prompt,
            int width,
            int height,
            int variantCount = 1,
            Dictionary<string, Stream>? references = null,
            CancellationToken cancellationToken = default);
    }
}
