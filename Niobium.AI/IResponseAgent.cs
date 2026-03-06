namespace Niobium.AI
{
    public interface IResponseAgent<TInput, TOutput> : IAgent
    {
        Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }
}
