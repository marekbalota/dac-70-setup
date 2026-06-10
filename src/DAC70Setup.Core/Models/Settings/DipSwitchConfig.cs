using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class DipSwitchConfig : SettingType61Enumerable
{
    public DipSwitchConfig() : base(nameof(PanelType.Setting), SettingType.DipSwitch8Mode, GetValues())
    {

    }

    private static SettingValue61Enumerable[] GetValues()
    {
        return
        [
            new SettingValue61Enumerable("Keep Aspect Ratio", [0x00,0x02,0x00,0x01,0x00,0x00]),
            new SettingValue61Enumerable("Pan Scan 16:9", [0x00,0x02,0x06,0x01,0x00,0x00])
        ];
    }
}
