using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class EdgeEnhanceConfig : SettingTypeC0
{
    public EdgeEnhanceConfig() : base(nameof(PanelType.Video), SettingType.EdgeEnhance, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("Disable", 0x00),
            new SettingValueC0("Min", 0x01),
            new SettingValueC0("Mid", 0x02),
            new SettingValueC0("Max", 0x03)
        ];
    }
}
