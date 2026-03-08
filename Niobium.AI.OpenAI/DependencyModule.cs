using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        public static IServiceCollection AddOpenAI(this IServiceCollection services)
        {
            _ = Niobium.AI.DependencyModule.AddAI(services);
            return services
                .AddSingleton(sp => new OpenAIClient(
                new ApiKeyCredential(Environment.GetEnvironmentVariable("OPENAI_KEY")!),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")!),
                    NetworkTimeout = TimeSpan.FromMinutes(15)
                }))
                .AddTransient<IChatClientFactory, OpenAIResponseClientFactory>();
        }
    }
}
