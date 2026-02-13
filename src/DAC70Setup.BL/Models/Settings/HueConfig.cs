using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;

namespace DAC70Setup.BL.Models.Settings;

public class HueConfig : SettingTypeC1
{
    public HueConfig() : base(nameof(PanelType.ProcAmp), SettingType.Hue)
    {

    }
}
