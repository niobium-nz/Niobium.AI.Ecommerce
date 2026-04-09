namespace Niobium.AI
{
    public interface IVideoProducer<TInput, TOutput> : IResponseGenerator<TInput, TOutput> where TInput : IVideoInstruction
    {
    }
}
