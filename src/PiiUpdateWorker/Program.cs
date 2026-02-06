using Azure.Messaging.ServiceBus;
using PiiUpdateWorker;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp => 
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

// Add Prometheus metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

// Expose /metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
