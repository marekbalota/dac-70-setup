using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Controls.SettingSelection;

public class SettingRowGroup : List<SettingRowItem>
{
    public SettingRowGroup(PanelType panel, IEnumerable<SettingRowItem> items) : base(items)
    {
        Panel = panel;
    }

    public PanelType Panel { get; }
}
