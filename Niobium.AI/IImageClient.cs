namespace Niobium.AI
{
    public interface IImageClient
    {
        Task<IEnumerable<BinaryData>> RunAsync(
            string conversationID,
            string prompt,
            int width,
            int height,
            int variantCount = 1,
            Dictionary<string, BinaryData>? references = null,
            CancellationToken cancellationToken = default);
    }
}
