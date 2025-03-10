using API.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Needed for Swagger UI
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
    app.UseSwagger(); // Enable Swagger JSON endpoint
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
