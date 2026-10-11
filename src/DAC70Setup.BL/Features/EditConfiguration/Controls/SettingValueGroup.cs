using System.Collections.ObjectModel;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Features.EditConfiguration.Controls;

public class SettingValueGroup : ObservableCollection<Dac70SettingValue>
{
    public SettingValueGroup(PanelType panel, IEnumerable<Dac70SettingValue> items)
        : base(items)
    {
        Panel = panel;
    }

    public PanelType Panel { get; }
}
