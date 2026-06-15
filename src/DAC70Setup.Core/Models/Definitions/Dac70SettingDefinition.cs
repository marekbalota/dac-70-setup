using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public abstract class Dac70SettingDefinition
{
    protected Dac70SettingDefinition(SettingKey key, PanelType panel, string name)
    {
        Key = key;
        Panel = panel;
        Name = name;
    }

    public SettingKey Key { get; }

    public PanelType Panel { get; }

    public string Name { get; }

    public abstract Dac70SettingValue CreateValue();

    public Dac70SettingValue CreateValue(ReadOnlySpan<byte> valueBytes)
    {
        Dac70SettingValue value = CreateValue();
        value.SetValueBytes(valueBytes);

        return value;
    }
}
