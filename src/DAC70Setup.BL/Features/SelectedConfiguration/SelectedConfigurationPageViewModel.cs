using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;

namespace DAC70Setup.BL.Features.SelectedConfiguration;

public class SelectedConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;

    public SelectedConfigurationPageViewModel(DataModel model, INavigationService navigationService)
    {
        Model = model;
        _navigationService = navigationService;

        GoToConfigurationsCommand = new DelegateCommand(async () =>
            await _navigationService.SelectTabAsync(nameof(ConfigurationsPage)));

        // TODO: write the selected configuration to the connected device.
        SendToDeviceCommand = new DelegateCommand(() => { });

        // TODO: save the selected configuration to a file the user picks.
        ExportConfigurationCommand = new DelegateCommand(() => { });

        // TODO: read the device's current configuration into a new configuration.
        DownloadFromDeviceCommand = new DelegateCommand(() => { });
    }

    public DataModel Model { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public DelegateCommand SendToDeviceCommand { get; }

    public DelegateCommand ExportConfigurationCommand { get; }

    public DelegateCommand DownloadFromDeviceCommand { get; }
}
