using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class InputSourceConfig : SettingTypeC0
{
    public InputSourceConfig() : base(nameof(PanelType.Video), SettingType.InputSource, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("VGA", 0x02),
            new SettingValueC0("HDMI", 0x01),
            new SettingValueC0("SDI", 0x04)
        ];
    }
}
