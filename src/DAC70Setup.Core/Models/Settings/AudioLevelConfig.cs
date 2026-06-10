using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class AudioLevelConfig : SettingTypeC0
{
    public AudioLevelConfig() : base(nameof(PanelType.Audio), SettingType.AudioLevel, GetValues())
    {

    }

    private static SettingValueC0[] GetValues()
    {
        return
        [
            new SettingValueC0("SMTPE(USA)", 0x00),
            new SettingValueC0("EBU(Europe)", 0x01)
        ];
    }
}
