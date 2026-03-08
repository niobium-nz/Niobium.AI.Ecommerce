using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Niobium.AI.Console;
using Niobium.AI.Ecommerce;
using Niobium.AI.OpenAI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? throw new InvalidOperationException("ServiceName is not set.");
var serviceVersion = builder.Configuration.GetValue<string>("ServiceVersion") ?? "1.0.0";
var applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATION_INSIGHTS_CONNECTION_STRING");

var otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        options.AddAzureMonitorLogExporter(options => options.ConnectionString = applicationInsightsConnectionString);
    }
    // Format log messages. This is default to false.
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
})
.SetMinimumLevel(LogLevel.Debug);

var tracerBuilder = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddHttpClientInstrumentation()
    .AddSource("Niobium.AI")
    .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry.
    .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry.
    .AddConsoleExporter();

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    _ = tracerBuilder.AddAzureMonitorTraceExporter(options => options.ConnectionString = applicationInsightsConnectionString);
}

if (!String.IsNullOrWhiteSpace(otlpEndpoint))
{
    _ = tracerBuilder.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
}

var tracerProvider = tracerBuilder.Build();
builder.Services.AddSingleton(tracerProvider);

var meterBuilder = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation()
    .AddMeter("*Microsoft.Agents.AI") // Agent Framework metrics
    .AddConsoleExporter();

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    _ = meterBuilder.AddAzureMonitorMetricExporter(options => options.ConnectionString = applicationInsightsConnectionString);
}

if (!String.IsNullOrWhiteSpace(otlpEndpoint))
{
    _ = meterBuilder.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
}

var meterProvider = meterBuilder.Build();
builder.Services.AddSingleton(meterProvider);

builder.Services.AddOpenAI();
builder.Services.AddEcommerce();
builder.Services.AddHostedService<WorkflowWorker>();
IHost host = builder.Build();
host.Run();
