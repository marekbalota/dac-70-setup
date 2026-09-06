namespace DAC70Setup.BL.Features.Settings;

/// <summary>
/// The theme the user picked, remembered across launches.
/// <see cref="AppTheme.Unspecified"/> means follow the OS.
/// </summary>
public static class AppThemePreference
{
    private const string ThemeKey = "app_theme";

    public static AppTheme Current
    {
        get => (AppTheme)Preferences.Default.Get(ThemeKey, (int)AppTheme.Unspecified);
        set
        {
            Preferences.Default.Set(ThemeKey, (int)value);

            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = value;
            }
        }
    }
}
