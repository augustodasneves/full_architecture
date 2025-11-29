using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using SupportAgent.Application.Interfaces;
using SupportAgent.Application.Services;
using SupportAgent.Infrastructure.Persistence;
using SupportAgent.Infrastructure.Persistence.Repositories;
using SupportAgent.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.EnableRetryOnFailure()));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost"));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Repositories
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

// Services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPiiUpdateHandler, PiiUpdateHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SupportAgent API V1");
        c.RoutePrefix = "api/swagger";
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply migrations at startup (for simplicity in this demo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
