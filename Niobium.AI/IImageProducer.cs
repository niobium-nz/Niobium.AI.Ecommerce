namespace Niobium.AI
{
    public interface IImageProducer : IResponseGenerator<ImageInstruction, IEnumerable<Uri>>
    {
    }
}
