// File: API/Program.cs

using API.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
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
builder.Services.ConfigureJwtAuthentication(builder.Configuration); // Register JWT authentication
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
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Add this line to enable authentication
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

