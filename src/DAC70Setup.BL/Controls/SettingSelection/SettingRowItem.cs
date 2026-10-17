using System.ComponentModel;
using DAC70Setup.BL.Controls.SettingSelection.Resources;
using DAC70Setup.Core;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.SettingSelection;

public class SettingRowItem : NotifyPropertyChangedBase, IDisposable
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

    private Dac70SettingValue? _configurationValue;
    public Dac70SettingValue? ConfigurationValue
    {
        get => _configurationValue;
        set
        {
            var oldValue = _configurationValue;
            SetProperty(ref _configurationValue, value, () =>
            {
                ObserveValue(oldValue, value);
                RaisePropertyChanged(nameof(IsIncluded));
                Refresh();
            });
        }
    }

    private Dac70SettingValue? _deviceValue;
    public Dac70SettingValue? DeviceValue
    {
        get => _deviceValue;
        set
        {
            var oldValue = _deviceValue;
            SetProperty(ref _deviceValue, value, () =>
            {
                ObserveValue(oldValue, value);
                Refresh();
            });
        }
    }

    public bool IsIncluded => ConfigurationValue != null;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private SettingComparison _comparison;
    public SettingComparison Comparison
    {
        get => _comparison;
        private set => SetProperty(ref _comparison, value);
    }

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

        RaisePropertyChanged(nameof(DeviceText));
    }

    public void Dispose()
    {
        ObserveValue(_configurationValue, null);
        ObserveValue(_deviceValue, null);
    }
}
