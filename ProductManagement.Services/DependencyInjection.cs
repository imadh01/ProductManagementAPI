using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Services.BusinessLogic;
using ProductManagement.Services.Mappings;

namespace ProductManagement.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServicesLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}