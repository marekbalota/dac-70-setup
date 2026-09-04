using DAC70Setup.Core;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Features.EditConfiguration.Controls;

public class SettingValueItem : NotifyPropertyChangedBase
{
    public SettingValueItem(Dac70SettingValue value)
    {
        Value = value;
    }

    public Dac70SettingValue Value { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
