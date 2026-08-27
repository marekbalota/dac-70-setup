using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public sealed class EnumSettingDefinition : Dac70SettingDefinition
{
    public EnumSettingDefinition(SettingKey key, PanelType panel, string name, IReadOnlyList<EnumOption> options)
        : this(key, panel, name, options, template: new byte[1], valueOffset: 0)
    {
    }

    public EnumSettingDefinition(SettingKey key, PanelType panel, string name, IReadOnlyList<EnumOption> options,
        byte[] template, int valueOffset)
        : base(key, panel, name)
    {
        Options = options;
        Template = template;
        ValueOffset = valueOffset;
    }

    public IReadOnlyList<EnumOption> Options { get; }

    internal byte[] Template { get; }

    internal int ValueOffset { get; }

    public override Dac70SettingValue CreateValue()
    {
        return new EnumSettingValue(this);
    }
}
