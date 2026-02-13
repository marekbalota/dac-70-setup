using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;

namespace DAC70Setup.BL.Models.Settings;

public class ContrastConfig : SettingTypeC1
{
    public ContrastConfig() : base(nameof(PanelType.ProcAmp), SettingType.Contrast)
    {

    }
}
