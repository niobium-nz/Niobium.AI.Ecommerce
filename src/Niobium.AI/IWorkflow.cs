namespace Niobium.AI
{
    public interface IWorkflow<TInput, TOutput> : IResponseGenerator<TInput, TOutput>
        where TInput : notnull
        where TOutput : class
    {
    }
}
