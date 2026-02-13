using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class VideoOutputConfig : SettingTypeC0
{
    public VideoOutputConfig() : base(nameof(PanelType.Video), SettingType.VideoOutputRange, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("Limit Range", 0x00),
            new SettingValueC0("Full Range", 0x01)
        ];
    }
}
