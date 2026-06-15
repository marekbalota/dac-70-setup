using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Core.Models.Values;

public abstract class Dac70SettingValue<TDefinition> : Dac70SettingValue
    where TDefinition : Dac70SettingDefinition
{
    protected Dac70SettingValue(TDefinition definition)
        : base(definition)
    {
    }

    public override TDefinition Definition => (TDefinition)base.Definition;
}
