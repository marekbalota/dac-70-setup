using System.Collections.ObjectModel;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Models;

public class SettingValueGroup : ObservableCollection<Dac70SettingValue>
{
    public SettingValueGroup(PanelType panel, IEnumerable<Dac70SettingValue> values)
        : base(values)
    {
        Panel = panel;
    }

    public PanelType Panel { get; }
}
