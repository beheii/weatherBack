using System;
using System.Collections.Generic;

namespace weatherapp.Models;

public partial class Wind
{
    public int WindId { get; set; }

    public int WeatherCurrentId { get; set; }

    public double? Speed { get; set; }

    public int? Deg { get; set; }

    public double? Gust { get; set; }

    public virtual WeatherCurrent WeatherCurrent { get; set; } = null!;
}
