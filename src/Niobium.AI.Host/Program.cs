using Microsoft.Extensions.Hosting;
using Niobium.AI.Ecommerce;
using Niobium.AI.Host;
using Niobium.AI.OpenAI;
using Niobium.AI.WebScraper.Firecrawl;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.ConfigureOpenTelemetry()
    .AddEcommerce()
    .AddOpenAI()
    .AddFirecrawl();

IHost host = builder.Build();
await host.RunAsync();