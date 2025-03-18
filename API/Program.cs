using API.Extensions;
using API.Middleware;
using API.Middlewares;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10485760,
            retainedFileCountLimit: 31);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureCors();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();  // Add your mappings profile here
}, typeof(Program).Assembly);
builder.Services.AddDbContext<SPSSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SPSS"))
        .EnableSensitiveDataLogging()); // EnableSensitiveDataLogging nếu cần thiết
var app = builder.Build();
if (app.Environment.IsDevelopment() || true) // Luôn bật Swagger
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty; // Đặt root URL để Swagger mở tại trang chính
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontendApp");
app.UseMiddleware<API.Middlewares.RequestResponseMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();
