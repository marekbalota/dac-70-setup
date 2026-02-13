using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class OsdHideConfig : SettingTypeC0
{
    public OsdHideConfig() : base(nameof(PanelType.Setting), SettingType.OsdHideMode, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("Disable", 0x00),
            new SettingValueC0("Enable", 0x01)
        ];
    }
}
