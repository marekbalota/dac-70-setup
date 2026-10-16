using System.ComponentModel;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Device;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.SelectedConfiguration;

public class SelectedConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly DeviceConnectionService _deviceConnectionService;

    public SelectedConfigurationPageViewModel(DataModel model, INavigationService navigationService,
        DeviceConnectionService deviceConnectionService, ILocalizationResourceManager localizationResourceManager)
    {
        Model = model;
        _navigationService = navigationService;
        _deviceConnectionService = deviceConnectionService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);

        GoToConfigurationsCommand = new DelegateCommand(async () =>
            await _navigationService.SelectTabAsync(nameof(ConfigurationsPage)));

        SendToDeviceCommand = new AsyncDelegateCommand(async () =>
        {
            if (Model.SelectedConfiguration is { } configuration)
            {
                await _deviceConnectionService.SendConfigurationAsync(configuration);
            }
        }, () => Model.SelectedConfiguration != null && _deviceConnectionService.CanSend);

        // TODO: save the selected configuration to a file the user picks.
        ExportConfigurationCommand = new DelegateCommand(() => { });

        SynchronizeWithDeviceCommand = new AsyncDelegateCommand(async () =>
        {
            await _deviceConnectionService.SyncAsync();
        }, () => _deviceConnectionService.CanSend);

        Model.PropertyChanged += OnSendConditionsChanged;
        _deviceConnectionService.PropertyChanged += OnSendConditionsChanged;
    }

    public DataModel Model { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public AsyncDelegateCommand SendToDeviceCommand { get; }

    public DelegateCommand ExportConfigurationCommand { get; }

    public AsyncDelegateCommand SynchronizeWithDeviceCommand { get; }

    private void OnSendConditionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        SendToDeviceCommand.RaiseCanExecuteChanged();
        SynchronizeWithDeviceCommand.RaiseCanExecuteChanged();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Model.PropertyChanged -= OnSendConditionsChanged;
        _deviceConnectionService.PropertyChanged -= OnSendConditionsChanged;
        DeviceStatus.Dispose();
    }
}
