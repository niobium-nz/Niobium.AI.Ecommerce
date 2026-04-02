using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Niobium.AI.BlobStorage;
using Niobium.AI.Console;
using Niobium.AI.Ecommerce;
using Niobium.AI.OpenAI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? throw new InvalidOperationException("ServiceName is not set.");
string serviceVersion = builder.Configuration.GetValue<string>("ServiceVersion") ?? "1.0.0";
string? applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATION_INSIGHTS_CONNECTION_STRING");

string? otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");

ResourceBuilder resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new Dictionary<string, object>
    {
        ["service.instance.id"] = Environment.MachineName,
        ["deployment.environment"] = "development"
    });

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    if (!String.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        options.AddAzureMonitorLogExporter(options => options.ConnectionString = applicationInsightsConnectionString);
    }
    // Format log messages. This is default to false.
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
})
.SetMinimumLevel(LogLevel.Debug);

TracerProviderBuilder tracerBuilder = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    /*.AddHttpClientInstrumentation()*/
    .AddSource("*Niobium.AI*")
    .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry.
    .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry.
    .AddConsoleExporter();

if (!String.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    tracerBuilder.AddAzureMonitorTraceExporter(options => options.ConnectionString = applicationInsightsConnectionString);
}

if (!String.IsNullOrWhiteSpace(otlpEndpoint))
{
    tracerBuilder.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
}

TracerProvider tracerProvider = tracerBuilder.Build();
builder.Services.AddSingleton(tracerProvider);

MeterProviderBuilder meterBuilder = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    /*.AddHttpClientInstrumentation()*/
    /*.AddRuntimeInstrumentation()*/
    .AddMeter("*Microsoft.Agents.AI") // Agent Framework metrics
    .AddConsoleExporter();

if (!String.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    meterBuilder.AddAzureMonitorMetricExporter(options => options.ConnectionString = applicationInsightsConnectionString);
}

if (!String.IsNullOrWhiteSpace(otlpEndpoint))
{
    meterBuilder.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
}

MeterProvider meterProvider = meterBuilder.Build();
builder.Services.AddSingleton(meterProvider);

builder.Services.AddBlobStorage();
builder.Services.AddOpenAI();
builder.Services.AddEcommerce();
builder.Services.AddHostedService<WorkflowWorker>();
IHost host = builder.Build();
host.Run();
