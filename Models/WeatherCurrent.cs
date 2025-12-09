using System;
using System.Collections.Generic;

namespace weatherapp.Models;

public partial class WeatherCurrent
{
    public int WeatherCurrentId { get; set; }

    public int CityId { get; set; }

    public int WeatherConditionId { get; set; }

    public double? Temperature { get; set; }

    public double? TemperatureMin { get; set; }

    public double? TemperatureMax { get; set; }

    public double? FeelsLike { get; set; }

    public int? Pressure { get; set; }

    public int? Humidity { get; set; }

    public int? Cloudiness { get; set; }

    public double? Visibility { get; set; }

    public long? TimeStamp { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Sun> Suns { get; set; } = new List<Sun>();

    public virtual WeatherCondition WeatherCondition { get; set; } = null!;

    public virtual ICollection<Wind> Winds { get; set; } = new List<Wind>();
}
