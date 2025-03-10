using API.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.ConfigureDbContext(builder.Configuration);

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();