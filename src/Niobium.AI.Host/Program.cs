using Microsoft.Extensions.Hosting;
using Niobium.AI.BlobStorage;
using Niobium.AI.Ecommerce;
using Niobium.AI.Host;
using Niobium.AI.OpenAI;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args)
    .ConfigureOpenTelemetry();

builder.Services
    .AddBlobStorage()
    .AddOpenAI()
    .AddEcommerce();

IHost host = builder.Build();
await host.RunAsync();