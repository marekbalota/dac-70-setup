using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;

namespace DAC70Setup.Core.Models.Settings;

public class BrightnessConfig : SettingTypeC1
{
    public BrightnessConfig() : base(nameof(PanelType.ProcAmp), SettingType.Brightness)
    {

    }
}
