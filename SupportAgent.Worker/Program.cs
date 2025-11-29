using Microsoft.EntityFrameworkCore;
using SupportAgent.Application.Interfaces;
using SupportAgent.Application.Services;
using SupportAgent.Infrastructure.Persistence;
using SupportAgent.Infrastructure.Persistence.Repositories;
using SupportAgent.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.EnableRetryOnFailure()));

// Repositories
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

// Services
builder.Services.AddScoped<IPiiUpdateHandler, PiiUpdateHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
