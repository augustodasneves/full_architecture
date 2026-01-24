using WhatsAppProxyApi.Models;
using WhatsAppProxyApi.Services;
using WhatsAppProxyApi.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure WhatsApp settings
builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection("WhatsApp"));

// Register WhatsApp Services
builder.Services.AddHttpClient<IBaileysClient, BaileysClient>();
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
