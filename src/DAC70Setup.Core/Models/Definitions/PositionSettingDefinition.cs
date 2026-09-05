using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class PositionSettingDefinition : Dac70SettingDefinition
{
    public PositionSettingDefinition(SettingKey key, PanelType panel, string name,
        int horizontalMinimum = 5, int horizontalMaximum = 600,
        int verticalMinimum = 5, int verticalMaximum = 100,
        SettingDependency? dependency = null)
        : base(key, panel, name)
    {
        HorizontalMinimum = horizontalMinimum;
        HorizontalMaximum = horizontalMaximum;
        VerticalMinimum = verticalMinimum;
        VerticalMaximum = verticalMaximum;
        Dependency = dependency;
    }

    public int HorizontalMinimum { get; }

    public int HorizontalMaximum { get; }

    public int VerticalMinimum { get; }

    public int VerticalMaximum { get; }

    /// <remarks>
    /// DAC-70 instruction manual, p.13: "Note 1: When "Input Source" set on "VGA",
    /// the Image Adjust can set Horizontal Position and Vertical Position."
    /// </remarks>
    public override SettingDependency? Dependency { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new PositionSettingValue(this);
    }
}
