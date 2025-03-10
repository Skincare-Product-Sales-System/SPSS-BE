using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Implementation;
using Repositories.Interface;
using Services.Implementation;
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
        services.AddScoped<ICancelReasonService, CancelReasonService>();
        return services;
    }

    public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SPSSContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SPSS"));
        });
        return services;
    }
}