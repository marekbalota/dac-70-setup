using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;

namespace DAC70Setup;

public partial class MainWindow
{
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        var application = Application.Current!;
        application.RequestedThemeChanged -= OnRequestedThemeChanged;

        if (Handler != null)
        {
            application.RequestedThemeChanged += OnRequestedThemeChanged;
            ChangeButtonColors();
        }
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        ChangeButtonColors();
    }

    private void ChangeButtonColors()
    {
        if (Handler?.PlatformView is not Microsoft.UI.Xaml.Window platformWindow || !AppWindowTitleBar.IsCustomizationSupported())
        {
            return;
        }

        platformWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var application = Application.Current!;
            string key = application.RequestedTheme == AppTheme.Dark ? "OnHeaderDark" : "OnHeaderLight";
            var foreground = (Color)application.Resources[key];

            var titleBar = platformWindow.AppWindow.TitleBar;
            titleBar.ButtonForegroundColor = foreground.ToWindowsColor();
            titleBar.ButtonHoverForegroundColor = foreground.ToWindowsColor();
            titleBar.ButtonPressedForegroundColor = foreground.ToWindowsColor();
            titleBar.ButtonInactiveForegroundColor = foreground.WithAlpha(0.6f).ToWindowsColor();
            titleBar.ButtonHoverBackgroundColor = foreground.WithAlpha(0.12f).ToWindowsColor();
            titleBar.ButtonPressedBackgroundColor = foreground.WithAlpha(0.2f).ToWindowsColor();
        });
    }
}
