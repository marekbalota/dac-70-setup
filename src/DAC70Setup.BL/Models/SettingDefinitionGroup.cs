using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Models;

public class SettingDefinitionGroup : List<Dac70SettingDefinition>
{
    public SettingDefinitionGroup(PanelType panel, IEnumerable<Dac70SettingDefinition> definitions)
        : base(definitions)
    {
        Panel = panel;
    }

    public PanelType Panel { get; }
}
