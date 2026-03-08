using Microsoft.Extensions.AI;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIChatClientFactory(OpenAIClient client) : IChatClientFactory
    {
        public IChatClient CreateChatClient(string model)
            => client.GetChatClient(model).AsIChatClient();
    }
}
