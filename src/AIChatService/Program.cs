using AIChatService.Services;
using AIChatService.Validators;
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

// HTTP Client for WhatsApp Proxy
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppHttpService>(client =>
{
    var whatsappProxyUrl = builder.Configuration["WhatsAppProxy:BaseUrl"] ?? "http://whatsapp-proxy-api:8080";
    client.BaseAddress = new Uri(whatsappProxyUrl);
});

// Validators
builder.Services.AddSingleton<PhoneValidator>();
builder.Services.AddSingleton<EmailValidator>();
builder.Services.AddSingleton<AddressValidator>();

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

app.MapControllers();

app.Run();
