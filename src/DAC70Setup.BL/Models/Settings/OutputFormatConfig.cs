using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class OutputFormatConfig : SettingTypeC0
{
    public OutputFormatConfig() : base(nameof(PanelType.Video), SettingType.OutputFormat, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("1920x1080 60.00p", 0x0f),
            new SettingValueC0("1920x1080 59.94p", 0x10),
            new SettingValueC0("1920x1080 50.00p ", 0x11),
            new SettingValueC0("1920x1080 30.00p ", 0x12),
            new SettingValueC0("1920x1080 29.97p", 0x13),
            new SettingValueC0("1920x1080 25.00p ", 0x14),
            new SettingValueC0("1920x1080 24.00p", 0x15),
            new SettingValueC0("1920x1080 23.98p", 0x16),
            new SettingValueC0("1920x1080 60.00i", 0x0c),
            new SettingValueC0("1920x1080 59.94i", 0x0d),
            new SettingValueC0("1920x1080 50.00i", 0x0e),
            new SettingValueC0("1280x720 60.00p", 0x06),
            new SettingValueC0("1280x720 59.94p", 0x07),
            new SettingValueC0("1280x720 50.00p", 0x08),
            new SettingValueC0("720x480 60.00p", 0x03),
            new SettingValueC0("720x480 59.94p", 0x04),
            new SettingValueC0("720x576 50.00p", 0x05),
            new SettingValueC0("720x480 59.94i", 0x01),
            new SettingValueC0("720x576 50.00i", 0x02)
        ];
    }
}
