using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;

namespace DAC70Setup.Core.Models.Settings;

public class SaturationConfig : SettingTypeC1
{
    public SaturationConfig() : base(nameof(PanelType.ProcAmp), SettingType.Saturation)
    {

    }
}
