using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;

namespace DAC70Setup.Core.Models.Settings;

public class HueConfig : SettingTypeC1
{
    public HueConfig() : base(nameof(PanelType.ProcAmp), SettingType.Hue)
    {

    }
}
