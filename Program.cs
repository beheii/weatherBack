using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<WeatherService>();

var app = builder.Build();

app.UseCors("AllowAngularApp");

app.MapGet("/api/weather/city", async (string city, WeatherService weatherService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"Received request for city: {city}");
        var weatherJson = await weatherService.GetWeatherByCityAsync(city);
        return Results.Content(weatherJson, "application/json");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error fetching weather data for {city}: {ex.Message}");
        
        return Results.Problem(
            detail: ex.Message,
            statusCode: 400
        );
    }
})
.WithName("GetWeatherByCity");


app.MapGet("/", () => "Weather app is running");

app.Run();

