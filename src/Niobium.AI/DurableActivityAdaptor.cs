using Microsoft.DurableTask;

namespace Niobium.AI
{
    internal class DurableActivityAdaptor<TInput, TOutput>(TaskOrchestrationContext context, string name) : IResponseGenerator<TInput, TOutput>
        where TInput : notnull
        where TOutput : class
    {
        public string Id => name;

        public async Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null)
            => await context.CallActivityAsync<TOutput>(name, input: input);
    }
}
