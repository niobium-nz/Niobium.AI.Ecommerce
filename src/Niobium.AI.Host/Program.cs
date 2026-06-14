using Microsoft.Extensions.Hosting;
using Niobium.AI;
using Niobium.AI.Ecommerce;
using Niobium.AI.Host;
using Niobium.AI.OpenAI;
using Niobium.AI.Playwright;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureOpenTelemetry()
    .AddEcommerce()
    .AddOpenAI()
    .AddPlaywright();

IHost host = builder.Build();
await host.RunAsync();