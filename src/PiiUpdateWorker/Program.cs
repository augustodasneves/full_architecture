using Azure.Messaging.ServiceBus;
using PiiUpdateWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(sp => 
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddHttpClient();

builder.Services.AddHostedService<Worker>();

// Add Prometheus scraping endpoint for the worker
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var host = builder.Build();

// Configure the worker to expose metrics on port 8080
host.UseOpenTelemetryPrometheusScrapingEndpoint();

host.Run();
