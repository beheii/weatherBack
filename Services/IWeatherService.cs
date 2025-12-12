namespace weatherapp.Services;

public interface IWeatherService
{
    Task<string> GetWeatherByCityAsync(string city);
}

