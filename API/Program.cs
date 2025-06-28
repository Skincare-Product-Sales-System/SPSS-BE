using API;
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

// In your Program.cs or Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()  // For development only - restrict this in production
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
    });
});
builder.Services.AddSignalR();
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
builder.Services.AddHttpClient();
builder.Services.AddScoped<VietQRService>();

var app = builder.Build();

// Apply migrations at startup
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<SPSSContext>();
    
//    // Apply pending migrations
//    Console.WriteLine("Applying pending migrations...");
//    dbContext.Database.Migrate();
//    Console.WriteLine("Migrations applied successfully!");
//}

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
app.UseCors("AllowAll");
app.UseMiddleware<API.Middlewares.RequestResponseMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    // Định nghĩa SignalR Hub
    endpoints.MapHub<ChatHub>("/chathub");
    endpoints.MapHub<TransactionHub>("/transactionhub");

    // Định nghĩa API endpoints
    endpoints.MapControllers();
});
app.MapControllers();
app.Run();
