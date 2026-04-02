namespace Niobium.AI
{
    public interface IImageInstruction
    {
        ImageForm Form { get; }

        Dictionary<string, BinaryData> References { get; }
    }
}
