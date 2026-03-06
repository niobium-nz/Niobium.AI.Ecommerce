using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.AI.Console;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHostedService<WorkflowWorker>();
IHost host = builder.Build();
host.Run();