using DAC70Setup.Core.Models.ConfigEnums;
using DAC70Setup.Core.Models.SettingValues;

namespace DAC70Setup.Core.Models.SettingTypes;

public abstract class SettingTypeC0 : NotifyPropertyChangedBase, IDac70Setting
{
    private readonly byte _settingType = 0xC0;
    private readonly byte _settingId;

    protected SettingTypeC0(string panelName, SettingType setting, SettingValueC0[] values)
    {
        PanelName = panelName;
        SettingName = setting.ToString();
        _settingId = (byte)setting;
        Values = values;
    }

    public string PanelName { get; }

    public string SettingName { get; }

    public SettingValueC0[] Values { get; }

    private SettingValueC0 _selectedValue = null!;
    public SettingValueC0 SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public byte[] GeyPayload()
    {
        throw new NotImplementedException();
    }
}
