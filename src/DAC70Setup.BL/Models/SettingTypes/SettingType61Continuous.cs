using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.SettingTypes;

public abstract class SettingType61Continuous : IDac70Setting
{
    private readonly byte _settingType = 0x61;
    private readonly byte _settingId;

    protected SettingType61Continuous(string panelName, SettingType setting, SettingValue61Continuous[] values)
    {
        PanelName = panelName;
        SettingName = setting.ToString();
        _settingId = (byte)setting;
        Values = values;
    }

    public string PanelName { get; }

    public string SettingName { get; }

    public SettingValue61Continuous[] Values { get; }

    public abstract byte[] GeyPayload();
}
