namespace Niobium.AI
{
    public interface IResponseGenerator<TInput, TOutput> : IExecutor
    {
        Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null);
    }
}
