using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class PositionSettingDefinition : Dac70SettingDefinition
{
    public PositionSettingDefinition(SettingKey key, PanelType panel, string name,
        int horizontalMinimum = 5, int horizontalMaximum = 600,
        int verticalMinimum = 5, int verticalMaximum = 100)
        : base(key, panel, name)
    {
        HorizontalMinimum = horizontalMinimum;
        HorizontalMaximum = horizontalMaximum;
        VerticalMinimum = verticalMinimum;
        VerticalMaximum = verticalMaximum;
    }

    public int HorizontalMinimum { get; }

    public int HorizontalMaximum { get; }

    public int VerticalMinimum { get; }

    public int VerticalMaximum { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new PositionSettingValue(this);
    }
}
