using System.Net.Http.Headers;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niobium.Ads.Analyst;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging();
builder.Services.AddHttpClient<AdsDiscoverer>(httpClient =>
{ 
    httpClient.Timeout = TimeSpan.FromMinutes(2);
    httpClient.BaseAddress = new Uri("https://niobiumadsmcpapp.mangosky-a7b92dc1.westus2.azurecontainerapps.io");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("CeYXCoG4U82EEITMpkDiTh1EOmi");
});

builder.Services
    .AddTransient<ProductNormalizer>()
    .AddTransient<CompetitionScout>()
    .AddTransient<ProductClusterer>()
    .AddTransient<KeywordsPlanner>()
    .AddTransient<ProductProfiler>()
    .AddTransient<AnalystWorkflow>()
    .AddSingleton(sp => new AIProjectClient(
        new Uri("https://whaneus2.services.ai.azure.com/api/projects/firstProject"),
        new DefaultAzureCredential(),
        new AIProjectClientOptions { NetworkTimeout = TimeSpan.FromMinutes(10) }));

builder.Services.AddHostedService<WorkflowWorker>();

IHost host = builder.Build();
host.Run();