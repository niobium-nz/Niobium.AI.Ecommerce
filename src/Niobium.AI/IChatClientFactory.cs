using Microsoft.Extensions.AI;

namespace Niobium.AI
{
    public interface IChatClientFactory
    {
        IChatClient CreateChatClient(string model);
    }
}
