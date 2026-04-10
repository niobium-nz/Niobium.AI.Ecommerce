using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Niobium.AI.Console
{
    internal static class BuilderExtensions
    {
        public static HostApplicationBuilder ConfigureOpenTelemetry(this HostApplicationBuilder builder)
        {
            string serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? throw new InvalidOperationException("ServiceName is not set.");
            string serviceVersion = builder.Configuration.GetValue<string>("ServiceVersion") ?? "1.0.0";
            string? applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATION_INSIGHTS_CONNECTION_STRING");

            string? otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");

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
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            })
            .SetMinimumLevel(LogLevel.Debug);

            OpenTelemetryBuilder otel = builder.Services.AddOpenTelemetry();
            otel.WithTracing(tracerBuilder =>
            {
                tracerBuilder.SetResourceBuilder(resourceBuilder)
                .AddHttpClientInstrumentation()
                .AddSource("Niobium.*")
                .AddSource("*Microsoft.Extensions.AI")
                .AddSource("*Microsoft.Extensions.Agents*");
            })
            .WithMetrics(meterBuilder =>
            {
                meterBuilder.SetResourceBuilder(resourceBuilder)
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("*Microsoft.Agents.AI");
            });

            if (!String.IsNullOrWhiteSpace(applicationInsightsConnectionString))
            {
                otel.UseAzureMonitorExporter(options => options.ConnectionString = applicationInsightsConnectionString);
            }

            if (!String.IsNullOrWhiteSpace(otlpEndpoint))
            {
                otel.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(otlpEndpoint));
            }

            return builder;
        }
    }
}
