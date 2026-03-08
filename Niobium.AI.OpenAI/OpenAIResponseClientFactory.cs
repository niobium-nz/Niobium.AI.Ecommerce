using Microsoft.Extensions.AI;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIResponseClientFactory(OpenAIClient client) : IChatClientFactory
    {
        public IChatClient CreateChatClient(string model)
            => client.GetResponsesClient(model).AsIChatClient();
    }
}
