using API.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;  // Add this

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => 
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()  // Logs to console
        .WriteTo.File(
            path: "logs/log-.txt",  // Logs to file with date
            rollingInterval: RollingInterval.Day,  // New file each day
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10485760, // 10MB
            retainedFileCountLimit: 31);  // Keep 31 days of logs
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1",
        Description = "A sample ASP.NET Core API",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your-email@example.com",
            Url = new Uri("https://example.com")
        }
    });
});
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseSerilogRequestLogging();  // Add this for request logging

app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();  // Complete the endpoints configuration
});

app.Run();

// Add this at the end to ensure proper shutdown of Serilog
public partial class Program { }  // Needed for Serilog