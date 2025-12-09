using System;
using System.Collections.Generic;

namespace weatherapp.Models;

public partial class WeatherCondition
{
    public int WeatherConditionId { get; set; }

    public string MainName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Icon { get; set; } = null!;

    public virtual ICollection<WeatherCurrent> WeatherCurrents { get; set; } = new List<WeatherCurrent>();
}
