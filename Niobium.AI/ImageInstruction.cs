namespace Niobium.AI
{
    public class ImageInstruction
    {
        public ImageForm Form { get; set; }

        public Dictionary<string, Stream> References { get; set; } = [];
    }
}
