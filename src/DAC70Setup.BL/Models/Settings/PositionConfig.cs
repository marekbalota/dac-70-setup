using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingTypes;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.Settings;

public class PositionConfig : SettingType61Continuous
{
    public PositionConfig() : base(nameof(PanelType.ImageAdjust), SettingType.Position, GetValues())
    {

    }

    private static SettingValue61Continuous[] GetValues()
    {
        return
        [
            new SettingValue61Continuous("Horizontal", 5, 600),
            new SettingValue61Continuous("Vertical", 5, 100)
        ];
    }

    public override byte[] GeyPayload()
    {
        throw new NotImplementedException();
    }
}
