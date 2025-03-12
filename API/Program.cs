using API.Extensions;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureJwtAuthentication(builder.Configuration); 
builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontendApp");
app.UseMiddleware<API.Middlewares.RequestResponseMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.Run();
