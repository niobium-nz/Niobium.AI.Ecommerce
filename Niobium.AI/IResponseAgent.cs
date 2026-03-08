namespace Niobium.AI
{
    public interface IResponseAgent<TInput, TOutput> : IAgent
    {
        Task<TOutput> GetResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }
}
