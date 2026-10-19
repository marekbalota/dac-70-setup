using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.Device;

public partial class DeviceSnapshot : ObservableObject
{
    private readonly Dictionary<SettingKey, Dac70SettingValue> _values = [];

    public DeviceSnapshot()
    {
        Values = _values.AsReadOnly();
    }

    public IReadOnlyDictionary<SettingKey, Dac70SettingValue> Values { get; }

    [ObservableProperty]
    public partial DateTime? LastFullSync { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDeviceInfo))]
    public partial Dac70FirmwareVersion? FirmwareVersion { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDeviceInfo))]
    public partial Dac70InputSignal? InputSignal { get; set; }

    public bool HasDeviceInfo => FirmwareVersion != null || InputSignal != null;

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

        OnPropertyChanged(nameof(Values));
    }

    public void CompleteFullSync(DateTime date)
    {
        LastFullSync = date;
    }
}
