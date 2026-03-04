using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.Ads.Analyst;
using Niobium.Ads.Analyst.Agents;
using Niobium.Ads.Analyst.AgentTools;
using OpenAI;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging();

builder.Services
    .AddSingleton<McpTools>()
    .AddTransient<IMetaAdsLibrary, TestAdsLibrary>()
    .AddTransient<AdsDiscoverer>()
    .AddTransient<ProductNormalizer>()
    .AddTransient<CompetitionScout>()
    .AddTransient<ProductClusterer>()
    .AddTransient<KeywordsPlanner>()
    .AddTransient<ProductProfiler>()
    .AddTransient<AnalystWorkflow>()
    .AddSingleton(sp => new OpenAIClient(
    new ApiKeyCredential(Environment.GetEnvironmentVariable("OPENAI_KEY")!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")!),
        NetworkTimeout = TimeSpan.FromMinutes(15)
    }));

builder.Services.AddHostedService<WorkflowWorker>();

IHost host = builder.Build();
host.Run();