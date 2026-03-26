namespace Niobium.AI
{
    public interface IResponseGenerator<TInput, TOutput> : IExecutor
    {
        Task<TOutput> GetResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }
}
