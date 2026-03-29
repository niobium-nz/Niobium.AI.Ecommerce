using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        public static IServiceCollection AddAI(this IServiceCollection services)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;
            services.AddTransient(typeof(UserInputAdaptor<>));
            return services;
        }
    }
}
