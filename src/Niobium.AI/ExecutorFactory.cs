using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    internal class ExecutorFactory(IServiceProvider serviceProvider)
    {
        public T Build<T>() where T : IExecutor => serviceProvider.GetRequiredService<T>();
    }
}
