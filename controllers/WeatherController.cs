using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using weatherapp.Services;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(WeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet("city")]
    public async Task<IActionResult> GetWeatherByCity([FromQuery] string city)
    {
        try
        {
            _logger.LogInformation($"Received request for city: {city}");
            var weatherJson = await _weatherService.GetWeatherByCityAsync(city);
            return Content(weatherJson, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching weather data for {city}: {ex.Message}");
            
            return Problem(
                detail: ex.Message,
                statusCode: 400
            );
        }
    }
}

