namespace Niobium.AI
{
    public interface IImageInstruction
    {
        ImageForm Form { get; }

        List<ImageReference> References { get; }
    }
}
