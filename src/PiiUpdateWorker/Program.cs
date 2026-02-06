using Shared.Telemetry;
using Azure.Messaging.ServiceBus;
using PiiUpdateWorker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomTelemetry(builder.Configuration, "PiiUpdateWorker");

builder.Services.AddSingleton(sp => 
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Expose /metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
