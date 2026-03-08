using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI.OpenAI
{
    public static class DependencyModule
    {
        public static IServiceCollection AddOpenAI(this IServiceCollection services)
        {
            _ = Niobium.AI.DependencyModule.AddAI(services);
            return services.AddTransient<IChatClientFactory, OpenAIChatClientFactory>();
        }
    }
}
