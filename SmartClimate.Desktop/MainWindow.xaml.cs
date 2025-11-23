using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using SmartClimate.Core;
using System.Collections.Generic;

namespace SmartClimate.Desktop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly EnvironmentModel _env;
    private readonly TemperatureSensor _tempSensor;
    private readonly HumiditySensor _humSensor;
    private readonly LightSensor _lightSensor;
    private readonly MotionSensor _motionSensor;
    private readonly HeaterActuator _heater;
    private readonly CoolerActuator _cooler;
    private readonly LampActuator _lamp;
    private readonly ClimateController _controller;

    private readonly DispatcherTimer _timer;
    private DateTime _lastUpdate = DateTime.Now;

    private bool _prevHeater;
    private bool _prevCooler;
    private bool _prevLamp;

    private ClimateMode _selectedMode = ClimateMode.Comfort;
    private readonly ObservableCollection<string> _events = new();

    public MainWindow()
    {
        InitializeComponent();

        _env = new EnvironmentModel();
        _tempSensor = new TemperatureSensor(_env);
        _humSensor = new HumiditySensor(_env);
        _lightSensor = new LightSensor(_env);
        _motionSensor = new MotionSensor(_env);
        _heater = new HeaterActuator(_env);
        _cooler = new CoolerActuator(_env);
        _lamp = new LampActuator(_env);

        _controller = new ClimateController(_tempSensor, _lightSensor, _motionSensor, _heater, _cooler, _lamp)
        {
            Mode = _selectedMode
        };

        DataContext = this;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += TimerOnTick;
        _timer.Start();

        AddEvent("Емулятор запущено. Режим: Comfort.");
    }

    private void TimerOnTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var dt = now - _lastUpdate;
        _lastUpdate = now;

        _controller.Step();
        _env.Update(dt);

        OnPropertyChanged(nameof(TimeOfDayText));
        OnPropertyChanged(nameof(TemperatureText));
        OnPropertyChanged(nameof(HumidityText));
        OnPropertyChanged(nameof(LightText));
        OnPropertyChanged(nameof(OccupantText));
        OnPropertyChanged(nameof(ActuatorsText));

        CheckActuatorChanges(now);
    }

    private void CheckActuatorChanges(DateTime now)
    {
        if (_prevHeater != _heater.IsOn ||
            _prevCooler != _cooler.IsOn ||
            _prevLamp != _lamp.IsOn)
        {
            _prevHeater = _heater.IsOn;
            _prevCooler = _cooler.IsOn;
            _prevLamp = _lamp.IsOn;

            var msg =
                $"[{now:HH:mm:ss}] T={_tempSensor.ReadValue():F1}°C, режим={SelectedMode}, " +
                $"обігрівач={BoolToUa(_heater.IsOn)}, " +
                $"кондиціонер={BoolToUa(_cooler.IsOn)}, " +
                $"світло={BoolToUa(_lamp.IsOn)}";

            AddEvent(msg);
        }
    }

    private void AddEvent(string text)
    {
        _events.Insert(0, text);          // нові події зверху
        if (_events.Count > 50)
            _events.RemoveAt(_events.Count - 1);
    }

    private static string BoolToUa(bool value) => value ? "увімкнено" : "вимкнено";

    // --------- властивості для прив'язки ---------

    public string TimeOfDayText => $"Віртуальний час: {_env.TimeOfDay:hh\\:mm}";

    public string TemperatureText => $"Температура: {_tempSensor.ReadValue():F1} °C";

    public string HumidityText => $"Вологість: {_humSensor.ReadValue():F0} %";

    public string LightText => $"Освітленість: {_lightSensor.ReadValue():F0} лк";

    public string OccupantText =>
        _motionSensor.ReadValue()
            ? "У приміщенні є людина"
            : "У приміщенні нікого немає";

    public string ActuatorsText =>
        $"Обігрівач: {BoolToUa(_heater.IsOn)}, " +
        $"Кондиціонер: {BoolToUa(_cooler.IsOn)}, " +
        $"Освітлення: {BoolToUa(_lamp.IsOn)}";
    
    public IReadOnlyDictionary<ClimateMode, string> ModeNames { get; } =
        new Dictionary<ClimateMode, string>
        {
            { ClimateMode.Comfort, "Комфорт" },
            { ClimateMode.Eco,     "Еко" },
            { ClimateMode.Away,   "Відсутність" }
        };

    // Цю властивість і буде бачити ComboBox
    public IEnumerable<KeyValuePair<ClimateMode, string>> Modes => ModeNames;


    public ClimateMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (_selectedMode == value) return;
            _selectedMode = value;
            _controller.Mode = value;
            OnPropertyChanged();
            AddEvent($"Режим змінено на: {_selectedMode}");
        }
    }

    public ObservableCollection<string> Events => _events;

    // --------- події UI ---------

    private void ToggleOccupantButton_OnClick(object sender, RoutedEventArgs e)
    {
        _env.ToggleOccupant();
        OnPropertyChanged(nameof(OccupantText));
    }

    // --------- INotifyPropertyChanged ---------

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
