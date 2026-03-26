namespace Niobium.AI
{
    public interface IWorkflow
    {
        string Id { get; }

        string Render();

        Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken);
    }

    public interface IWorkflow<TInput, TOutput> : IWorkflow
        where TInput : class
        where TOutput : class
    {
        Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken);
    }
}
