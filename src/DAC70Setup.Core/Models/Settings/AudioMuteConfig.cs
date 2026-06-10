using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class AudioMuteConfig : SettingTypeC0
{
    public AudioMuteConfig() : base(nameof(PanelType.Audio), SettingType.AudioMute, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("OFF", 0x00),
            new SettingValueC0("ON", 0x01)
        ];
    }
}
