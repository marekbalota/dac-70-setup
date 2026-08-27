using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class SizeSettingDefinition : Dac70SettingDefinition
{
    public SizeSettingDefinition(SettingKey key, PanelType panel, string name,
        int minimum = 80, int maximum = 100)
        : base(key, panel, name)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public int Minimum { get; }

    public int Maximum { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new SizeSettingValue(this);
    }
}
