using WhatsAppProxyApi.Models;
using WhatsAppProxyApi.Services;
using WhatsAppProxyApi.Clients;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
        // Customizing Retry: 3 attempts with exponential backoff
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.Delay = TimeSpan.FromSeconds(2);

        // Customizing Circuit Breaker: 
        // If 50% of requests fail in a 30s window, open the circuit for 30s.
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<IWhatsAppService, BaileysWhatsAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
