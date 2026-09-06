using DAC70Setup.BL.Features.Settings;

namespace DAC70Setup;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppThemePreference.Current;
    }
}
