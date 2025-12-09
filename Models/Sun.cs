using System;
using System.Collections.Generic;

namespace weatherapp.Models;

public partial class Sun
{
    public int SunId { get; set; }

    public int WeatherCurrentId { get; set; }

    public long Sunrise { get; set; }

    public long Sunset { get; set; }

    public virtual WeatherCurrent WeatherCurrent { get; set; } = null!;
}
