using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class RangeSettingDefinition : Dac70SettingDefinition
{
    public RangeSettingDefinition(SettingKey key, PanelType panel, string name,
        int minimum = byte.MinValue, int maximum = byte.MaxValue)
        : base(key, panel, name)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public int Minimum { get; }

    public int Maximum { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new RangeSettingValue(this);
    }
}
