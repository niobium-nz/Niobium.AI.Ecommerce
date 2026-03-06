using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace Niobium.AI
{
    public static class DependencyModule
    {
        public static IServiceCollection AddAI(this IServiceCollection services) => services
            .AddSingleton(sp => new OpenAIClient(
            new ApiKeyCredential(Environment.GetEnvironmentVariable("OPENAI_KEY")!),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")!),
                NetworkTimeout = TimeSpan.FromMinutes(15)
            }));
    }
}
