using CommunityToolkit.Maui.PlatformConfiguration.AndroidSpecific;

namespace DAC70Setup.BL;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            CurrentPage?.Handler?.UpdateValue(NavigationBar.ColorProperty.PropertyName);
        }
    }
}
