using System.ComponentModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Controls.Confirmation;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Features.SelectedConfiguration.Popups.ExportConfiguration;
using DAC70Setup.BL.Features.SelectedConfiguration.Resources;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.Navigation;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.SelectedConfiguration;

public partial class SelectedConfigurationPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly IPopupService _popupService;

    public SelectedConfigurationPageViewModel(ConfigurationsService configurationsService,
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

    [RelayCommand]
    private async Task ExportConfigurationAsync()
    {
        if (ConfigurationsService.SelectedConfiguration is not { } configuration)
        {
            return;
        }

        await _popupService.ShowPopupAsync<ExportConfigurationPopupViewModel>(Shell.Current, options: null,
            shellParameters: new Dictionary<string, object>
            {
                [ExportConfigurationPopupViewModel.ConfigurationParameter] = configuration
            });
    }

    [RelayCommand(CanExecute = nameof(CanUseDevice))]
    private async Task DownloadFromDeviceAsync()
    {
        await _deviceConnectionService.SyncAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUseDevice))]
    private async Task FactoryResetAsync()
    {
        var popupResult = await _popupService.ShowPopupAsync<ConfirmationPopupViewModel, bool>(
            Shell.Current,
            shellParameters: new Dictionary<string, object>
            {
                [ConfirmationPopupViewModel.TitleKeyParameter] = nameof(SelectedConfigurationPageSR.FactoryReset),
                [ConfirmationPopupViewModel.MessageKeyParameter] = nameof(SelectedConfigurationPageSR.FactoryResetConfirmMessage),
                [ConfirmationPopupViewModel.ConfirmTextKeyParameter] = nameof(SelectedConfigurationPageSR.FactoryResetConfirmAccept)
            });

        if (popupResult.WasDismissedByTappingOutsideOfPopup || popupResult.Result != true)
        {
            return;
        }

        await _deviceConnectionService.FactoryResetAsync();
    }

    private bool CanUseDevice()
    {
        return _deviceConnectionService.CanSend;
    }

    private void OnSendConditionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        SendToDeviceCommand.NotifyCanExecuteChanged();
        DownloadFromDeviceCommand.NotifyCanExecuteChanged();
        FactoryResetCommand.NotifyCanExecuteChanged();
    }
}
