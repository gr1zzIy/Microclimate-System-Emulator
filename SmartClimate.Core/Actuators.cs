namespace SmartClimate.Core;

public abstract class Actuator
{
    protected readonly EnvironmentModel Environment;

    protected Actuator(EnvironmentModel env) => Environment = env;

    public abstract string Name { get; }

    public abstract bool IsOn { get; protected set; }

    public abstract void SetState(bool on);
}

public class HeaterActuator : Actuator
{
    public HeaterActuator(EnvironmentModel env) : base(env) { }

    public override string Name => "Heater";

    public override bool IsOn { get; protected set; }

    public override void SetState(bool on)
    {
        IsOn = on;
        Environment.HeatingPower = on ? 1.0 : 0.0;
    }
}

public class CoolerActuator : Actuator
{
    public CoolerActuator(EnvironmentModel env) : base(env) { }

    public override string Name => "AC";

    public override bool IsOn { get; protected set; }

    public override void SetState(bool on)
    {
        IsOn = on;
        Environment.CoolingPower = on ? 1.0 : 0.0;
    }
}

public class LampActuator : Actuator
{
    public LampActuator(EnvironmentModel env) : base(env) { }

    public override string Name => "Lamp";

    public override bool IsOn { get; protected set; }

    public override void SetState(bool on)
    {
        IsOn = on;
        Environment.LightPower = on ? 1.0 : 0.0;
    }
}
