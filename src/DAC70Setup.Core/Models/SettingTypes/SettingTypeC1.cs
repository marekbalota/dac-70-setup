using DAC70Setup.Core.Models.ConfigEnums;

namespace DAC70Setup.Core.Models.SettingTypes;

public abstract class SettingTypeC1 : NotifyPropertyChangedBase, IDac70Setting
{
    private readonly byte _settingType = 0xC1;
    private readonly byte _settingId;

    protected SettingTypeC1(string panelName, SettingType setting)
    {
        PanelName = panelName;
        SettingName = setting.ToString();
        _settingId = (byte)setting;
    }

    public string PanelName { get; }

    public string SettingName { get; }

    public int MinimumValue => 0;

    public int MaximumValue => 255;

    private byte _selectedValue;
    public byte SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public byte[] GeyPayload()
    {
        throw new NotImplementedException();
    }
}
