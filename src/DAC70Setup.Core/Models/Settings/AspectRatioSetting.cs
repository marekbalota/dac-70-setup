using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class AspectRatioSetting : SettingTypeC0
{
    public AspectRatioSetting() : base(nameof(PanelType.Video), SettingType.AspectRatio, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("Keep Aspect Ratio", 0x00),
            new SettingValueC0("Full Screen (16:9)", 0x01),
            new SettingValueC0("Dot By Dot", 0x02),
            new SettingValueC0("4:3", 0x03),
            new SettingValueC0("14:9", 0x04),
            new SettingValueC0("Underscan", 0x05),
            new SettingValueC0("Pan Scan 16:9", 0x06)
        ];
    }
}
