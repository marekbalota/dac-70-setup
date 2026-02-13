using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class Enable3GConfig : SettingType61Enumerable
{
    public Enable3GConfig() : base(nameof(PanelType.Video), SettingType.Enable3GLevelB, GetValues())
    {

    }

    private static SettingValue61Enumerable[] GetValues()
    {
        return
        [
            new SettingValue61Enumerable("Disable", [0x00,0x01,0x00,0x00,0x00,0x00]),
            new SettingValue61Enumerable("Enable", [0x00,0x01,0x01,0x00,0x00,0x00])
        ];
    }
}
