namespace Niobium.AI
{
    public interface IVideoAgent<T> : IResponseAgent<T, Stream> where T : IVideoInstruction
    {
    }
}
