using System.Text;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IProductStatusService, ProductStatusService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
    
    public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero // Recommended to reduce the default 5-minute clock skew
                };
            });

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