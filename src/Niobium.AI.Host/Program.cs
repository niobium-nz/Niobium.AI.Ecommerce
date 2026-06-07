using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.AI.BlobStorage;
using Niobium.AI.Ecommerce;
using Niobium.AI.Host;
using Niobium.AI.OpenAI;
using Niobium.AI.Playwright;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args)
    .ConfigureOpenTelemetry();

builder.Services
    .AddBlobStorage()
    .AddOpenAI(builder.Configuration.GetSection("OpenAI").Bind)
    .AddEcommerce()
    .AddPlaywright(builder.Configuration.GetSection("Playwright").Bind);

IHost host = builder.Build();
await host.RunAsync();