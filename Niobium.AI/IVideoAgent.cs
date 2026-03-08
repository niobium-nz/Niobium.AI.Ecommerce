namespace Niobium.AI
{
    public interface IVideoAgent<TInput, TOutput> : IAgent
        where TOutput : IResponseWithVideo
    {
        Task<TOutput> GetVideoAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }
}
