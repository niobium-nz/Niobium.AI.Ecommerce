namespace Niobium.AI
{
    public interface IImageClientFactory
    {
        IImageClient CreateClient(string model, string? provider = null);
    }
}
