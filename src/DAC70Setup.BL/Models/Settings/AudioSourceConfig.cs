using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class AudioSourceConfig : SettingTypeC0
{
    public AudioSourceConfig() : base(nameof(PanelType.Audio), SettingType.AudioSource, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("Internal", 0x01),
            new SettingValueC0("HDMI", 0x02),
            new SettingValueC0("SDI", 0x03),
            new SettingValueC0("L/R Audio", 0x00)
        ];
    }
}
