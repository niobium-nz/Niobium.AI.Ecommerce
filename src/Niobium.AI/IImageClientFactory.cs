namespace Niobium.AI
{
    public interface IImageClientFactory
    {
        IImageClient CreateClient(string model);
    }
}
