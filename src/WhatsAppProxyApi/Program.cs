using WhatsAppProxyApi.Models;
using WhatsAppProxyApi.Services;
using WhatsAppProxyApi.Clients;
using Polly;
using Shared.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomTelemetry(builder.Configuration, "WhatsAppProxyApi");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure WhatsApp settings
builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection("WhatsApp"));

// Register WhatsApp Services
builder.Services.AddHttpClient<IBaileysClient, BaileysClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<IWhatsAppService, BaileysWhatsAppService>();
builder.Services.AddScoped<WhatsAppProxyApi.Security.WhatsAppSignatureValidator>();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{builder.Configuration["WhatsApp:BaileysServiceUrl"]}/status"), name: "baileys-service");

var app = builder.Build();

// Configure Health Check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
