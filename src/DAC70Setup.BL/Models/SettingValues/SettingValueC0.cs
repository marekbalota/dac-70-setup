namespace DAC70Setup.BL.Models.SettingValues;

public class SettingValueC0
{
    public SettingValueC0(string name, byte value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public byte Value { get; }
}
