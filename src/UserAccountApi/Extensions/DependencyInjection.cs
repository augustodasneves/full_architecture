using Microsoft.EntityFrameworkCore;
using UserAccountApi.Data;
using UserAccountApi.Repositories;
using UserAccountApi.Services;

namespace UserAccountApi.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
