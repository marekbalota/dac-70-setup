using DAC70Setup.BL.Models.ConfigEnums;
using DAC70Setup.BL.Models.SettingValues;

namespace DAC70Setup.BL.Models.SettingTypes;

public abstract class SettingType61Enumerable : BindableBase, IDac70Setting
{
    private readonly byte _settingType = 0x61;
    private readonly byte _settingId;

    protected SettingType61Enumerable(string panelName, SettingType setting, SettingValue61Enumerable[] values)
    {
        PanelName = panelName;
        SettingName = setting.ToString();
        _settingId = (byte)setting;
        Values = values;
    }

    public string PanelName { get; }

    public string SettingName { get; }

    public SettingValue61Enumerable[] Values { get; }

    private SettingValue61Enumerable _selectedValue;
    public SettingValue61Enumerable SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public byte[] GeyPayload()
    {
        throw new NotImplementedException();
    }
}
