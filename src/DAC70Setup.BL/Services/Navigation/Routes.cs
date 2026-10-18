namespace DAC70Setup.BL.Services.Navigation;

public static class Routes
{
    public const string ConfigurationsRoot = "configurations";

    public const string SettingsRoot = "settings";

    public const string SelectedConfigurationSegment = "selected";

    public const string EditConfigurationSegment = "edit";

    public const string ConfigurationListSegment = "list";

    public const string EditSetting = "editsetting";

    public const string SelectedConfiguration = "//" + ConfigurationsRoot + "/" + SelectedConfigurationSegment;

    public const string ConfigurationList = "//" + ConfigurationsRoot + "/" + ConfigurationListSegment;
}
