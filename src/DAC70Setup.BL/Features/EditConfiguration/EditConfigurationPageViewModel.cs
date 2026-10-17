using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Controls.SettingSelection;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditConfiguration;

public class EditConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;

    public EditConfigurationPageViewModel(DataModel model, INavigationService navigationService,
        IConfigurationsService configurationsService, DeviceConnectionService deviceConnectionService,
        ILocalizationResourceManager localizationResourceManager)
    {
        Model = model;
        _navigationService = navigationService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);

        SettingSelection = new SettingSelectionViewModel(model, configurationsService, deviceConnectionService,
            localizationResourceManager, OpenSettingAsync);

        GoToConfigurationsCommand = new DelegateCommand(async () =>
            await _navigationService.SelectTabAsync(nameof(ConfigurationsPage)));
    }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public DataModel Model { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    public SettingSelectionViewModel SettingSelection { get; }

    private async Task OpenSettingAsync(Dac70SettingValue value)
    {
        if (IsBusy || Model.SelectedConfiguration is not { } configuration)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var parameters = new NavigationParameters
            {
                { EditSettingNavigationParameters.Configuration, configuration },
                { EditSettingNavigationParameters.SettingValue, value }
            };

            await _navigationService.NavigateAsync(nameof(EditSettingPage), parameters);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        SettingSelection.Dispose();
        DeviceStatus.Dispose();
    }
}
