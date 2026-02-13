using DAC70Setup.BL.Models.ConfigEnums;

namespace DAC70Setup.BL.Models.SettingTypes;

public abstract class SettingTypeC1 : BindableBase, IDac70Setting
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
