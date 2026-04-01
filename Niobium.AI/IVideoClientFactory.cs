namespace Niobium.AI
{
    public interface IVideoClientFactory
    {
        IVideoClient CreateClient(string model);
    }
}
