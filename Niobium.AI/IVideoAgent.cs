namespace Niobium.AI
{
    public interface IVideoAgent<T> : IAgent
    {
        Task<Uri> RunAsync(string conversationID, T input, CancellationToken cancellationToken);
    }
}
