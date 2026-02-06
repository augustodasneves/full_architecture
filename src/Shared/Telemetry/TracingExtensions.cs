using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Configuration;

namespace Shared.Telemetry;

public static class TracingExtensions
{
    public static IServiceCollection AddCustomTelemetry(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddSource("Azure.Messaging.ServiceBus")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://jaeger:4317");
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddMeter("OpenTelemetry.Instrumentation.AspNetCore")
                .AddMeter("OpenTelemetry.Instrumentation.Http")
                .AddMeter("OpenTelemetry.Instrumentation.Runtime")
                .AddMeter("OpenTelemetry.Instrumentation.Process")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                // Map OTel metric names to what the legacy dashboard expects
                // Prometheus exporter appends unit and _total, so we use base names here
                .AddView("process.memory.usage", "process_private_memory_bytes")
                .AddView("process.memory.virtual", "process_virtual_memory_bytes")
                .AddView("process.cpu.time", "process_cpu")
                .AddPrometheusExporter());

        return services;
    }
}
