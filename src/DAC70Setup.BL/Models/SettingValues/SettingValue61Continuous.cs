namespace DAC70Setup.BL.Models.SettingValues;

public class SettingValue61Continuous : BindableBase
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
