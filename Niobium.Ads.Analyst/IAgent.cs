namespace Niobium.Ads.Analyst
{
    public interface IAgent<TInput, TOutput> : IAgent
    {
        Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }

    public interface IAgent
    {
        string Name { get; }

        Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken);
    }
}
