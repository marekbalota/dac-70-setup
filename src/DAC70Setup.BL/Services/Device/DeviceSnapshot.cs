using DAC70Setup.Core;
using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.Device;

public class DeviceSnapshot : NotifyPropertyChangedBase
{
    private readonly Dictionary<SettingKey, Dac70SettingValue> _values = [];

    public DeviceSnapshot()
    {
        Values = _values.AsReadOnly();
    }

    public IReadOnlyDictionary<SettingKey, Dac70SettingValue> Values { get; }

    private DateTime? _lastFullSync;
    public DateTime? LastFullSync
    {
        get => _lastFullSync;
        private set => SetProperty(ref _lastFullSync, value);
    }

    public void SetValue(Dac70SettingValue value)
    {
        var valueBytes = value.GetValueBytes();

        if (_values.TryGetValue(value.Definition.Key, out var existing))
        {
            existing.SetValueBytes(valueBytes);
        }
        else
        {
            _values[value.Definition.Key] = value.Definition.CreateValue(valueBytes);
        }

        RaisePropertyChanged(nameof(Values));
    }

    public void CompleteFullSync(DateTime date)
    {
        LastFullSync = date;
    }
}
