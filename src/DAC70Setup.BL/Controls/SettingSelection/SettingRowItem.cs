using System.ComponentModel;
using DAC70Setup.BL.Controls.SettingSelection.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.SettingSelection;

public partial class SettingRowItem : ObservableObject, IDisposable
{
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public SettingRowItem(Dac70SettingDefinition definition, ILocalizationResourceManager localizationResourceManager)
    {
        Definition = definition;
        _localizationResourceManager = localizationResourceManager;

        DeviceText = new LocalizedString(() =>
        {
            if (DeviceValue?.DisplayValueText is { } valueText)
            {
                return valueText;
            }
            else if (Definition.IsReadable)
            {
                return _localizationResourceManager[nameof(SettingSelectionViewSR.SettingNotSynced)];
            }
            else
            {
                return _localizationResourceManager[nameof(SettingSelectionViewSR.SettingNotReadable)];
            }
        });
    }

    public Dac70SettingDefinition Definition { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIncluded))]
    public partial Dac70SettingValue? ConfigurationValue { get; set; }

    partial void OnConfigurationValueChanged(Dac70SettingValue? oldValue, Dac70SettingValue? newValue)
    {
        ObserveValue(oldValue, newValue);
        Refresh();
    }

    [ObservableProperty]
    public partial Dac70SettingValue? DeviceValue { get; set; }

    partial void OnDeviceValueChanged(Dac70SettingValue? oldValue, Dac70SettingValue? newValue)
    {
        ObserveValue(oldValue, newValue);
        Refresh();
    }

    public bool IsIncluded => ConfigurationValue != null;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial SettingComparison Comparison { get; private set; }

    public LocalizedString DeviceText { get; }

    private void ObserveValue(Dac70SettingValue? oldValue, Dac70SettingValue? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= OnValueChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += OnValueChanged;
        }
    }

    private void OnValueChanged(object? sender, PropertyChangedEventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        var configurationValue = ConfigurationValue;
        var deviceValue = DeviceValue;

        if (configurationValue != null && deviceValue != null)
        {
            if (configurationValue.GetValueBytes().SequenceEqual(deviceValue.GetValueBytes()))
            {
                Comparison = SettingComparison.Match;
            }
            else
            {
                Comparison = SettingComparison.Differs;
            }
        }
        else
        {
            Comparison = SettingComparison.Unknown;
        }

        OnPropertyChanged(nameof(DeviceText));
    }

    public void Dispose()
    {
        ObserveValue(ConfigurationValue, null);
        ObserveValue(DeviceValue, null);
    }
}
