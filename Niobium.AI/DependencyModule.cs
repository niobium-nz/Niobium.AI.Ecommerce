using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    public static class DependencyModule
    {
        private volatile static bool loaded = false;

        public static IServiceCollection AddAI(this IServiceCollection services)
        {
            if (!loaded)
            {
                loaded = true;
            }

            return services;
        }
    }
}
