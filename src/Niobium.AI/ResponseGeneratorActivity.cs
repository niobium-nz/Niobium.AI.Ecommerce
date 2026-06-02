using Microsoft.DurableTask;

namespace Niobium.AI
{
    [DurableTask]
    internal class ResponseGeneratorActivity<TResponseGenerator, TInput, TOutput>(ExecutorFactory factory)
        : TaskActivity<TInput, TOutput>
            where TResponseGenerator : IResponseGenerator<TInput, TOutput>
            where TInput : notnull
            where TOutput : class
    {
        public override Task<TOutput> RunAsync(TaskActivityContext context, TInput input)
            => factory.Build<TResponseGenerator>().RunAsync(input);
    }
}
