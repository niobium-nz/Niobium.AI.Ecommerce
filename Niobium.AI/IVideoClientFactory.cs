namespace Niobium.AI
{
    public interface IVideoClientFactory
    {
        IVideoClient CreateVideoClient(string model);
    }
}
