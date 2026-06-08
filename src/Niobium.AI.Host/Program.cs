using Microsoft.Extensions.Hosting;
using Niobium.AI;
using Niobium.AI.BlobStorage;
using Niobium.AI.Ecommerce;
using Niobium.AI.Host;
using Niobium.AI.OpenAI;
using Niobium.AI.Playwright;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureOpenTelemetry()
    .AddAI()
    .AddOpenAI()
    .AddPlaywright()
    .AddBlobStorage()
    .AddEcommerce();

IHost host = builder.Build();
await host.RunAsync();