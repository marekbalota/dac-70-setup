using AsyncAwaitBestPractices;
using DAC70Setup.BL;
using DAC70Setup.BL.Features.Settings;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;

namespace DAC70Setup;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    private bool _servicesStarted;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;

        UserAppTheme = AppThemePreference.Current;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new MainWindow(_serviceProvider.GetRequiredService<AppShell>());

        if (!_servicesStarted)
        {
            _servicesStarted = true;
            StartServices();
        }

        return window;
    }

    private void StartServices()
    {
        var configurationsService = _serviceProvider.GetRequiredService<ConfigurationsService>();
        var deviceConnectionService = _serviceProvider.GetRequiredService<DeviceConnectionService>();

        configurationsService.InitializeAsync().SafeFireAndForget();
        deviceConnectionService.RefreshDevicesAsync().SafeFireAndForget();
    }
}
