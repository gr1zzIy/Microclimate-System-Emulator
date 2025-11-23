namespace SmartClimate.Core;

public abstract class Sensor<T>
{
    protected readonly EnvironmentModel Environment;

    protected Sensor(EnvironmentModel env) => Environment = env;

    public abstract string Name { get; }

    public abstract T ReadValue();
}

public class TemperatureSensor : Sensor<double>
{
    public TemperatureSensor(EnvironmentModel env) : base(env) { }

    public override string Name => "TempSensor";

    public override double ReadValue() => Environment.TemperatureC;
}

public class HumiditySensor : Sensor<double>
{
    public HumiditySensor(EnvironmentModel env) : base(env) { }

    public override string Name => "HumiditySensor";

    public override double ReadValue() => Environment.HumidityPercent;
}

public class LightSensor : Sensor<double>
{
    public LightSensor(EnvironmentModel env) : base(env) { }

    public override string Name => "LightSensor";

    public override double ReadValue() => Environment.LightLux;
}

public class MotionSensor : Sensor<bool>
{
    public MotionSensor(EnvironmentModel env) : base(env) { }

    public override string Name => "MotionSensor";

    public override bool ReadValue() => Environment.HasOccupant;
}
