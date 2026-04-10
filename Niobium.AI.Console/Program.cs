using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.AI.BlobStorage;
using Niobium.AI.Console;
using Niobium.AI.Ecommerce;
using Niobium.AI.OpenAI;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args)
    .ConfigureOpenTelemetry();

builder.Services
    .AddBlobStorage()
    .AddOpenAI()
    .AddEcommerce()
    .AddHostedService<WorkflowWorker>();

await builder.Build().RunAsync();