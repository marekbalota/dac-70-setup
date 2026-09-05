using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class SizeSettingDefinition : Dac70SettingDefinition
{
    public SizeSettingDefinition(SettingKey key, PanelType panel, string name,
        int minimum = 80, int maximum = 100,
        SettingDependency? dependency = null)
        : base(key, panel, name)
    {
        Minimum = minimum;
        Maximum = maximum;
        Dependency = dependency;
    }

    public int Minimum { get; }

    public int Maximum { get; }

    /// <remarks>
    /// DAC-70 instruction manual, p.13: "Note2: When "Aspect Ratio" set on "Under Scan",
    /// the Image Adjust can set Horizontal Size and Vertical Size."
    /// </remarks>
    public override SettingDependency? Dependency { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new SizeSettingValue(this);
    }
}
