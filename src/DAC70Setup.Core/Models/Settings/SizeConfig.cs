using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingTypes;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.Settings;

public class SizeConfig : SettingType61Continuous
{
    public SizeConfig() : base(nameof(PanelType.ImageAdjust), SettingType.Size, GetValues())
    {

    }

    private static SettingValue61Continuous[] GetValues()
    {
        return
        [
            new SettingValue61Continuous("Horizontal", 80, 100),
            new SettingValue61Continuous("Vertical", 80, 100)
        ];
    }

    public override byte[] GeyPayload()
    {
        throw new NotImplementedException();
    }
}
