using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;

namespace DAC70Setup.Core.Models.Settings;

public class ContrastConfig : SettingTypeC1
{
    public ContrastConfig() : base(nameof(PanelType.ProcAmp), SettingType.Contrast)
    {

    }
}
