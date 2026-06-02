using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    public static class DependencyModule
    {
        private static volatile bool loaded = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection AddAI(this IServiceCollection services)
        {
            if (loaded)
            {
                return services;
            }

            loaded = true;

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            return services.AddSingleton(sp => new ExecutorFactory(sp))
                .ConfigureAI(callingAssembly);
        }
    }
}
