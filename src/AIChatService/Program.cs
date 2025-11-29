using AIChatService.Services;
using Azure.Messaging.ServiceBus;
using Shared.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost"));

// Service Bus
builder.Services.AddSingleton(sp => 
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

// HTTP Client for LLM
builder.Services.AddHttpClient<ILLMService, LLMService>();

// Domain Services
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<FlowEngine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseAuthorization();

app.MapControllers();

app.Run();
