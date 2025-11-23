namespace SmartClimate.Core;

public enum ClimateMode
{
    Comfort,
    Eco,
    Away
}

public class ClimateController
{
    private readonly TemperatureSensor _temp;
    private readonly LightSensor _light;
    private readonly MotionSensor _motion;
    private readonly HeaterActuator _heater;
    private readonly CoolerActuator _cooler;
    private readonly LampActuator _lamp;

    public ClimateMode Mode { get; set; } = ClimateMode.Comfort;

    public ClimateController(
        TemperatureSensor temp,
        LightSensor light,
        MotionSensor motion,
        HeaterActuator heater,
        CoolerActuator cooler,
        LampActuator lamp)
    {
        _temp = temp;
        _light = light;
        _motion = motion;
        _heater = heater;
        _cooler = cooler;
        _lamp = lamp;
    }

    public void Step()
    {
        var t = _temp.ReadValue();
        var hasPerson = _motion.ReadValue();
        var light = _light.ReadValue();

        double minT;
        double maxT;
        double darkThreshold;

        switch (Mode)
        {
            case ClimateMode.Eco:
                minT = 19;
                maxT = 23;
                darkThreshold = 120;
                break;

            case ClimateMode.Away:
                // ширший діапазон – економія
                minT = 16;
                maxT = 28;
                darkThreshold = 80;
                break;

            default: // Comfort
                minT = 21;
                maxT = 24;
                darkThreshold = 200;
                break;
        }

        // керування обігрівачем/кондиціонером
        if (t < minT)
        {
            _heater.SetState(true);
            _cooler.SetState(false);
        }
        else if (t > maxT)
        {
            _heater.SetState(false);
            _cooler.SetState(true);
        }
        else
        {
            _heater.SetState(false);
            _cooler.SetState(false);
        }

        // світло: у режимі Away лампу не вмикаємо
        var wantLight = hasPerson && Mode != ClimateMode.Away && light < darkThreshold;
        _lamp.SetState(wantLight);
    }
}
