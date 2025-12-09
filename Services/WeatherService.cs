using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.Models;

namespace weatherapp.Services;

public class WeatherService
{
    private readonly WeatherDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;
    
    public WeatherService(
        WeatherDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<string> GetWeatherByCityAsync(string city)
    {
        // STEP 1: Check Database for cached data
        var cityEntity = await _context.Cities
            .FirstOrDefaultAsync(c => c.Name.ToLower() == city.ToLower());
        
        if (cityEntity != null)
        {
            // Check for recent weather data (within last 30 minutes)
            var cachedWeather = await _context.WeatherCurrents
                .Include(w => w.City)
                .Include(w => w.WeatherCondition)
                .Include(w => w.Winds)
                .Include(w => w.Suns)
                .Where(w => w.CityId == cityEntity.CityId 
                         && w.TimeStamp > ((DateTimeOffset)DateTime.UtcNow.AddMinutes(-30)).ToUnixTimeSeconds())
                .OrderByDescending(w => w.TimeStamp)
                .FirstOrDefaultAsync();
            
            if (cachedWeather != null)
            {
                _logger.LogInformation($"Returning cached weather for {city} from database");
                return ConvertToJson(cachedWeather);
            }
        }
        
        // STEP 2: Not in DB or outdated - Fetch from API
        _logger.LogInformation($"Fetching fresh weather for {city} from API");
        var weatherJson = await FetchWeatherFromApiAsync(city);
        
        // STEP 3: Save to Database
        await SaveWeatherToDatabaseAsync(weatherJson);
        
        // STEP 4: Return to user
        return weatherJson;
    }
    
    private async Task SaveWeatherToDatabaseAsync(string weatherJson)
    {

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Starting to save weather data to database");
            
            var jsonDoc = JsonDocument.Parse(weatherJson);
            var root = jsonDoc.RootElement;
            
          
            var cityName = root.GetProperty("name").GetString() ?? "Unknown";
            var countryCode = root.GetProperty("sys").GetProperty("country").GetString();
            var timezoneData = root.TryGetProperty("timezone", out var tz) ? tz.GetInt32() : (int?)null;


            _logger.LogInformation($"Processing city: {cityName}, timezone: {timezoneData}, country: {countryCode}");
            
            
            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());
            
            if (city == null)
            {
                city = new City
                {
                    Name = cityName,
                    Code = countryCode,
                    Timezone = timezoneData  
                };
                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"City created with ID: {city.CityId}, timezone: {city.Timezone}");
            }
          
            
            // Extract weather condition
            var weatherArray = root.GetProperty("weather");
            var firstWeather = weatherArray[0];
            var mainName = firstWeather.GetProperty("main").GetString() ?? "";
            var description = firstWeather.GetProperty("description").GetString() ?? "";
            var icon = firstWeather.GetProperty("icon").GetString() ?? "";
            
            
            var weatherCondition = await _context.WeatherConditions
                .FirstOrDefaultAsync(wc => wc.MainName == mainName 
                                         && wc.Description == description);
            
            if (weatherCondition == null)
            {
                weatherCondition = new WeatherCondition
                {
                    MainName = mainName,
                    Description = description,
                    Icon = icon
                };
                _context.WeatherConditions.Add(weatherCondition);
                await _context.SaveChangesAsync();
            }
            
            // Extract main weather data
            var main = root.GetProperty("main");
            var clouds = root.GetProperty("clouds");
            var wind = root.GetProperty("wind");
            var sys = root.GetProperty("sys");
            
            var weatherCurrent = new WeatherCurrent
            {
                CityId = city.CityId,
                WeatherConditionId = weatherCondition.WeatherConditionId,
                Temperature = main.TryGetProperty("temp", out var temp) ? temp.GetDouble() : null,
                TemperatureMin = main.TryGetProperty("temp_min", out var tempMin) ? tempMin.GetDouble() : null,
                TemperatureMax = main.TryGetProperty("temp_max", out var tempMax) ? tempMax.GetDouble() : null,
                FeelsLike = main.TryGetProperty("feels_like", out var feelsLike) ? feelsLike.GetDouble() : null,
                Pressure = main.TryGetProperty("pressure", out var pressure) ? pressure.GetInt32() : null,
                Humidity = main.TryGetProperty("humidity", out var humidity) ? humidity.GetInt32() : null,
                Cloudiness = clouds.TryGetProperty("all", out var all) ? all.GetInt32() : null,
                Visibility = root.TryGetProperty("visibility", out var visibility) ? visibility.GetInt32() / 1000.0 : null,
                TimeStamp = root.GetProperty("dt").GetInt64()
            };
            
            _context.WeatherCurrents.Add(weatherCurrent);
            await _context.SaveChangesAsync();
        
            var windEntity = new Wind
            {
                WeatherCurrentId = weatherCurrent.WeatherCurrentId,
                Speed = wind.TryGetProperty("speed", out var speed) ? speed.GetDouble() : null,
                Deg = wind.TryGetProperty("deg", out var deg) ? deg.GetInt32() : null,
                Gust = wind.TryGetProperty("gust", out var gust) ? gust.GetDouble() : null
            };
            _context.Winds.Add(windEntity);
            
            var sunEntity = new Sun
            {
                WeatherCurrentId = weatherCurrent.WeatherCurrentId,
                Sunrise = sys.GetProperty("sunrise").GetInt64(),
                Sunset = sys.GetProperty("sunset").GetInt64()
            };
            _context.Suns.Add(sunEntity);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Weather data successfully saved to database for {cityName}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error saving weather data to database. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
            throw; 
        }
    }
    
    private string ConvertToJson(WeatherCurrent weatherCurrent)
    {
        // Convert database entity back to JSON format matchin  API
        var json = new
        {
            name = weatherCurrent.City?.Name,
            weather = new[]
            {
                new
                {
                    id = weatherCurrent.WeatherCondition?.WeatherConditionId ?? 0,
                    main = weatherCurrent.WeatherCondition?.MainName ?? "",
                    description = weatherCurrent.WeatherCondition?.Description ?? "",
                    icon = weatherCurrent.WeatherCondition?.Icon ?? ""
                }
            },
            main = new
            {
                temp = weatherCurrent.Temperature,
                feels_like = weatherCurrent.FeelsLike,
                temp_min = weatherCurrent.TemperatureMin,
                temp_max = weatherCurrent.TemperatureMax,
                pressure = weatherCurrent.Pressure,
                humidity = weatherCurrent.Humidity
            },
            clouds = new
            {
                all = weatherCurrent.Cloudiness
            },
            wind = weatherCurrent.Winds.FirstOrDefault() != null ? new
            {
                speed = weatherCurrent.Winds.First().Speed,
                deg = weatherCurrent.Winds.First().Deg,
                gust = weatherCurrent.Winds.First().Gust
            } : null,
            sys = weatherCurrent.Suns.FirstOrDefault() != null ? new
            {
                country = weatherCurrent.City?.Code,
                sunrise = weatherCurrent.Suns.First().Sunrise,
                sunset = weatherCurrent.Suns.First().Sunset
            } : null,
            visibility = weatherCurrent.Visibility.HasValue ? (int)(weatherCurrent.Visibility.Value * 1000) : 10000,
            dt = weatherCurrent.TimeStamp ?? 0,
            timezone = weatherCurrent.City?.Timezone, 
            id = weatherCurrent.CityId,
            cod = 200
        };
        
        return JsonSerializer.Serialize(json);
    }
    
    private async Task<string> FetchWeatherFromApiAsync(string city)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        var baseUrl = _configuration["OpenWeatherMap:BaseUrl"];
        var url = $"{baseUrl}/weather?q={city}&appid={apiKey}&units=metric";
        
        var response = await httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"OpenWeatherMap API error for city '{city}': Status {response.StatusCode}, Response: {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"City '{city}' not found. Please check the spelling and try again.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Invalid API key. Please check the OpenWeatherMap API configuration.");
            }
            else
            {
                throw new Exception($"Failed to fetch weather data for '{city}'. API returned status code: {(int)response.StatusCode}");
            }
        }
        
        return await response.Content.ReadAsStringAsync();
    }
}

