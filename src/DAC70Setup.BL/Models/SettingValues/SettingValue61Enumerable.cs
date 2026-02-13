namespace DAC70Setup.BL.Models.SettingValues;

public class SettingValue61Enumerable
{
    public SettingValue61Enumerable(string name, byte[] value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public byte[] Value { get; }
}
