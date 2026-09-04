using System.Collections.ObjectModel;
using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Features.EditConfiguration.Controls;

public class SettingValueGroup : ObservableCollection<SettingValueItem>
{
    public SettingValueGroup(PanelType panel, IEnumerable<SettingValueItem> items)
        : base(items)
    {
        Panel = panel;
    }

    public PanelType Panel { get; }
}
