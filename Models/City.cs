using System;
using System.Collections.Generic;

namespace weatherapp.Models;

public partial class City
{
    public int CityId { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public int? Timezone { get; set; }

    public virtual ICollection<WeatherCurrent> WeatherCurrents { get; set; } = new List<WeatherCurrent>();
}
