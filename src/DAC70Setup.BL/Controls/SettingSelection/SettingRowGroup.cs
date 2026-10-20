namespace DAC70Setup.BL.Controls.SettingSelection;

public class SettingRowGroup : List<SettingRowItem>
{
    public SettingRowGroup(string panelName, IEnumerable<SettingRowItem> items) : base(items)
    {
        PanelName = panelName;
    }

    public string PanelName { get; }
}
