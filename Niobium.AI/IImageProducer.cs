namespace Niobium.AI
{
    public interface IImageProducer<T> : IResponseGenerator<T, IEnumerable<BinaryData>> where T : IImageInstruction
    {
    }
}
