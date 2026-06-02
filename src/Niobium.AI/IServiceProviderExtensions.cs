using System.Reflection;
using System.Runtime.CompilerServices;
using DurableTask.Core;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Agents.AI.Workflows;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client.AzureManaged;
using Microsoft.DurableTask.Worker;
using Microsoft.DurableTask.Worker.AzureManaged;
using Microsoft.Extensions.DependencyInjection;

namespace Niobium.AI
{
    public static class IServiceProviderExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection ConfigureAI(this IServiceCollection services, Assembly? callingAssembly = null)
        {
            callingAssembly ??= Assembly.GetCallingAssembly();
            Type[] typesFromCallingAssembly = [.. callingAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)];

            IEnumerable<Type> executors = typesFromCallingAssembly.Where(t => typeof(IExecutor).IsAssignableFrom(t) || typeof(Executor).IsAssignableFrom(t));
            foreach (Type executor in executors)
            {
                services.AddTransient(executor);

                if (typeof(IExecutor).IsAssignableFrom(executor))
                {
                    services.AddTransient(typeof(IExecutor), executor);
                }
            }

            string dtsConnectionString = Environment.GetEnvironmentVariable("DTS_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DTS_CONNECTION_STRING environment variable is not set.");

            return services.ConfigureDurableAgents(
                options =>
                {
                    Dictionary<string, Type> workflowDefinitions = typesFromCallingAssembly.Where(t => typeof(GenericResponseAgent).IsAssignableFrom(t)).ToDictionary(t => t.Name, t => t);
                    foreach (string name in workflowDefinitions.Keys)
                    {
                        options.AddAIAgentFactory(name, sp => ((GenericResponseAgent)sp.GetRequiredService(workflowDefinitions[name])).GetOrCreateAgent());
                    }
                },
                clientBuilder: builder =>
                {
                    builder.UseDurableTaskScheduler(dtsConnectionString);
                },
                workerBuilder: builder =>
                {
                    builder.UseDurableTaskScheduler(dtsConnectionString);

                    builder.AddTasks(registry =>
                    {
                        // register response generator implementations by locating types that implement IResponseGenerator<,>
                        // and mapping them to the generic ResponseGeneratorActivity<TGenerator, TRequest, TResponse>
                        Type responseGeneratorInterfaceOpen = typeof(IResponseGenerator<,>);
                        Type responseGeneratorActivityOpen = typeof(ResponseGeneratorActivity<,,>);
                        Type genericResponseAgentType = typeof(GenericResponseAgent);
                        IEnumerable<Type> responseGenerators = typesFromCallingAssembly.Where(t =>
                            !genericResponseAgentType.IsAssignableFrom(t)
                            && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == responseGeneratorInterfaceOpen));

                        foreach (Type generator in responseGenerators)
                        {
                            Type interfaceType = generator.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == responseGeneratorInterfaceOpen);
                            Type[] genericArgs = interfaceType.GetGenericArguments();
                            Type activityType = responseGeneratorActivityOpen.MakeGenericType(generator, genericArgs[0], genericArgs[1]);
                            registry.AddActivity(generator.Name, activityType);
                        }

                        IEnumerable<Type> activities = typesFromCallingAssembly.Where(t => typeof(TaskActivity).IsAssignableFrom(t));
                        foreach (Type activity in activities)
                        {
                            registry.AddActivity(activity);
                        }

                        IEnumerable<Type> orchestrators = typesFromCallingAssembly.Where(t => typeof(ITaskOrchestrator).IsAssignableFrom(t));
                        foreach (Type orchestrator in orchestrators)
                        {
                            registry.AddOrchestrator(orchestrator);
                        }
                    });
                });
        }
    }
}
