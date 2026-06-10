using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class HdmiOutputConfig : SettingTypeC0
{
    public HdmiOutputConfig() : base(nameof(PanelType.Video), SettingType.HdmiOutputColor, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("YUV 444", 0x01),
            new SettingValueC0("YUV 422", 0x03),
            new SettingValueC0("RGB 444", 0x00)
        ];
    }
}
