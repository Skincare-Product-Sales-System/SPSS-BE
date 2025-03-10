using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.Implementation;
using Repositories.Interface;
using Services;
using Services.Interface;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }

    public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SPSS"));
        });
        return services;
    }
}