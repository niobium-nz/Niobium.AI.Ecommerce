namespace Niobium.AI
{
    public interface IVideoProducer<T> : IResponseGenerator<T, Stream> where T : IVideoInstruction
    {
    }
}
