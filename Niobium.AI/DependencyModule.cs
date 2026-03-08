using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    public static class DependencyModule
    {
        public static IServiceCollection AddAI(this IServiceCollection services)
            => services;
    }
}
