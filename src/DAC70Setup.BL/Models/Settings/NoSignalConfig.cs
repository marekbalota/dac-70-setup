using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class NoSignalConfig : SettingType61Enumerable
{
    public NoSignalConfig() : base(nameof(PanelType.Video), SettingType.NoSignalMode, GetValues())
    {

    }

    private static SettingValue61Enumerable[] GetValues()
    {
        return
        [
            new SettingValue61Enumerable("Black Screen", [0x00,0x01,0x00,0x01,0x00,0x00]),
            new SettingValue61Enumerable("Blue Screen", [0x00,0x01,0x01,0x00,0x00,0x00]),
            new SettingValue61Enumerable("Disable Output", [0x00,0x01,0x02,0x00,0x00,0x00])
        ];
    }
}
