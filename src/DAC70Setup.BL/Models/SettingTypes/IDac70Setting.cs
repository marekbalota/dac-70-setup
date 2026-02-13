namespace DAC70Setup.BL.Models.SettingTypes
{
    public interface IDac70Setting
    {
        string PanelName { get; }
        string SettingName { get; }

        byte[] GeyPayload();
    }
}
