using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.Navigation;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.SelectedConfiguration;

public partial class SelectedConfigurationPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly DeviceConnectionService _deviceConnectionService;

    public SelectedConfigurationPageViewModel(ConfigurationsService configurationsService,
        INavigationService navigationService,
        DeviceConnectionService deviceConnectionService, ILocalizationResourceManager localizationResourceManager)
    {
        ConfigurationsService = configurationsService;
        _navigationService = navigationService;
        _deviceConnectionService = deviceConnectionService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);

        ConfigurationsService.PropertyChanged += OnSendConditionsChanged;
        _deviceConnectionService.PropertyChanged += OnSendConditionsChanged;
    }

    public ConfigurationsService ConfigurationsService { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    [RelayCommand]
    private Task GoToConfigurationsAsync()
    {
        return _navigationService.NavigateToAsync(Routes.ConfigurationList);
    }

    [RelayCommand(CanExecute = nameof(CanSendToDevice))]
    private async Task SendToDeviceAsync()
    {
        if (ConfigurationsService.SelectedConfiguration is { } configuration)
        {
            await _deviceConnectionService.SendConfigurationAsync(configuration);
        }
    }

    private bool CanSendToDevice()
    {
        return ConfigurationsService.SelectedConfiguration != null && _deviceConnectionService.CanSend;
    }

    // TODO: save the selected configuration to a file the user picks.
    [RelayCommand]
    private void ExportConfiguration()
    {
    }

    [RelayCommand(CanExecute = nameof(CanSynchronizeWithDevice))]
    private async Task SynchronizeWithDeviceAsync()
    {
        await _deviceConnectionService.SyncAsync();
    }

    private bool CanSynchronizeWithDevice()
    {
        return _deviceConnectionService.CanSend;
    }

    private void OnSendConditionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        SendToDeviceCommand.NotifyCanExecuteChanged();
        SynchronizeWithDeviceCommand.NotifyCanExecuteChanged();
    }
}
