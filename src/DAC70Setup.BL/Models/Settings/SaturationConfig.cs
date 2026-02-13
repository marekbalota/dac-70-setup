using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;

namespace DAC70Setup.BL.Models.Settings;

public class SaturationConfig : SettingTypeC1
{
    public SaturationConfig() : base(nameof(PanelType.ProcAmp), SettingType.Saturation)
    {

    }
}
