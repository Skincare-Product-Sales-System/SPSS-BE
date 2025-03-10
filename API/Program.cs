using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// builder.Services.AddDbContext<Repositories.DbContext>(options =>
// {
//     options.UseSqlServer(configuration.GetConnectionString("BadmintonBazaarDb"));
// });

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

