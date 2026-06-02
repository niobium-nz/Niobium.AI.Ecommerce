namespace Niobium.AI
{
    public interface IImageProducer<TInput, TOutput>
        : IResponseGenerator<TInput, TOutput> where TInput : IImageInstruction
    {
    }
}
