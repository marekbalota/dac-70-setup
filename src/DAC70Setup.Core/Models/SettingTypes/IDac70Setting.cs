namespace DAC70Setup.Core.Models.SettingTypes;

public interface IDac70Setting
{
    string PanelName { get; }
    string SettingName { get; }
    byte[] GeyPayload();
}
