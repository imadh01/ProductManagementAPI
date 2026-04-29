using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Repository.Data;
using ProductManagement.Repository.Services;

namespace ProductManagement.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositoryServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductRepo, ProductRepo>();

        return services;
    }
}