using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;
using DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.Navigation;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations;

public partial class ConfigurationsPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly IPopupService _popupService;

    public ConfigurationsPageViewModel(ConfigurationsService configurationsService,
        INavigationService navigationService,
        DeviceConnectionService deviceConnectionService,
        IPopupService popupService,
        ILocalizationResourceManager localizationResourceManager)
    {
        ConfigurationsService = configurationsService;
        _navigationService = navigationService;
        _deviceConnectionService = deviceConnectionService;
        _popupService = popupService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);
    }

    public ConfigurationsService ConfigurationsService { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteConfigurationCommand))]
    public partial Configuration? SelectedConfiguration { get; set; }

    partial void OnSelectedConfigurationChanged(Configuration? oldValue, Configuration? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
        }

        if (newValue != null)
        {
            newValue.IsSelected = true;
        }
    }

    [RelayCommand]
    private async Task OpenConfigurationAsync(Configuration configuration)
    {
        ConfigurationsService.SelectConfiguration(configuration);

        await _navigationService.NavigateToAsync(Routes.SelectedConfiguration);
    }

    [RelayCommand]
    private void ToggleSelection(Configuration configuration)
    {
        SelectedConfiguration = SelectedConfiguration == configuration ? null : configuration;
    }

    [RelayCommand]
    private async Task CreateConfigurationAsync()
    {
        var popupResult = await _popupService
            .ShowPopupAsync<CreateConfigurationPopupViewModel, CreateConfigurationResult>(Shell.Current);

        if (popupResult.WasDismissedByTappingOutsideOfPopup || popupResult.Result is not { } result)
        {
            return;
        }

        if (!result.IsFromDevice)
        {
            await ConfigurationsService.CreateNewConfigurationAsync(result.Name);

            return;
        }

        if (_deviceConnectionService.SelectedDevice?.Snapshot.LastFullSync == null
            && !await _deviceConnectionService.SyncAsync())
        {
            return;
        }

        if (_deviceConnectionService.SelectedDevice?.Snapshot is { } snapshot)
        {
            await ConfigurationsService.CreateFromDeviceAsync(result.Name, snapshot);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteConfiguration))]
    private async Task DeleteConfigurationAsync()
    {
        if (SelectedConfiguration is not { } configuration)
        {
            return;
        }

        var popupResult = await _popupService.ShowPopupAsync<DeleteConfigurationPopupViewModel, bool>(
            Shell.Current,
            shellParameters: new Dictionary<string, object>
            {
                [DeleteConfigurationPopupViewModel.ConfigurationNameParameter] = configuration.Name
            });

        if (popupResult.WasDismissedByTappingOutsideOfPopup || popupResult.Result != true)
        {
            return;
        }

        await ConfigurationsService.DeleteConfigurationAsync(configuration);

        SelectedConfiguration = null;
    }

    private bool CanDeleteConfiguration()
    {
        return SelectedConfiguration != null;
    }
}
