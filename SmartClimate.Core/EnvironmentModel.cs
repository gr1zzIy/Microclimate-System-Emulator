public class EnvironmentModel
{
    public double TemperatureC { get; private set; } = 22.0;
    public double HumidityPercent { get; private set; } = 45.0;
    public double LightLux { get; private set; } = 150.0;
    public bool HasOccupant { get; private set; }

    // віртуальний час доби
    public TimeSpan TimeOfDay { get; private set; } = TimeSpan.FromHours(12);

    // “вуличні” умови
    public double OutdoorTemperatureC { get; private set; } = 18.0;
    public double OutdoorLightLux { get; private set; } = 5000.0;

    // вплив виконавчих пристроїв
    internal double HeatingPower { get; set; }
    internal double CoolingPower { get; set; }
    internal double LightPower { get; set; }

    private readonly Random _rnd = new();

    public void ToggleOccupant() => HasOccupant = !HasOccupant;

    public void Update(TimeSpan dt)
    {
        var seconds = dt.TotalSeconds;

        // 1) крутимо віртуальний час
        TimeOfDay = TimeOfDay.Add(dt);
        if (TimeOfDay >= TimeSpan.FromDays(1))
            TimeOfDay -= TimeSpan.FromDays(1);

        var hours = TimeOfDay.TotalHours;

        // 2) модель вуличної температури (мін ~12° вночі, макс ~30° вдень)
        OutdoorTemperatureC = 20 + 8 * Math.Sin(2 * Math.PI * (hours - 15) / 24);
        OutdoorTemperatureC = Math.Clamp(OutdoorTemperatureC, 12, 30);

        // 3) модель освітленості на вулиці (0 вночі, пік вдень)
        var daylightFactor = Math.Max(0, Math.Sin(Math.PI * (hours - 6) / 12)); // >0 між 6:00 та 18:00
        OutdoorLightLux = 25000 * daylightFactor;

        // 4) температура в кімнаті тягнеться до вуличної
        var driftToOutdoor = (OutdoorTemperatureC - TemperatureC) * 0.01 * seconds;
        TemperatureC += driftToOutdoor;

        // невеликий шум
        TemperatureC += (0.01 * _rnd.NextDouble() - 0.005) * seconds;

        // вплив обігрівача/кондиціонера
        TemperatureC += HeatingPower * 0.06 * seconds;
        TemperatureC -= CoolingPower * 0.06 * seconds;

        // 5) вологість – людина дає більше “пари”
        var humidityBase = HasOccupant ? 0.01 : 0.003;
        HumidityPercent += humidityBase * _rnd.NextDouble() * seconds;
        HumidityPercent = Math.Clamp(HumidityPercent, 20, 80);

        // 6) освітленість усередині – частина вуличної + лампа
        LightLux = OutdoorLightLux * 0.1 + LightPower * 300 + _rnd.Next(-10, 11);
        LightLux = Math.Clamp(LightLux, 0, 30000);
    }
}