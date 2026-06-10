using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class HdmiInputConfig : SettingTypeC0
{
    public HdmiInputConfig() : base(nameof(PanelType.Video), SettingType.HdmiInputEq, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("LOW", 0x02),
            new SettingValueC0("MIDDLE", 0x00),
            new SettingValueC0("HIGH", 0x01)
        ];
    }
}
