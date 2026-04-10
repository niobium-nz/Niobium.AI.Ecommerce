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
            List<ImageReference>? references = null,
            CancellationToken cancellationToken = default);
    }
}
