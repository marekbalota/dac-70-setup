namespace DAC70Setup.Core.Models.SettingValues;

public class SettingValue61Continuous : NotifyPropertyChangedBase
{
    public SettingValue61Continuous(string name, int minimumValue, int maximumValue)
    {
        Name = name;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    public string Name { get; }

    public int MinimumValue { get; }

    public int MaximumValue { get; }

    private short _selectedValue;
    public short SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
}
