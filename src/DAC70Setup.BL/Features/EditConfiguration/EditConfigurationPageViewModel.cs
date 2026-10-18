using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Controls.SettingSelection;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.Navigation;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditConfiguration;

public partial class EditConfigurationPageViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public EditConfigurationPageViewModel(ConfigurationsService configurationsService,
        INavigationService navigationService,
        DeviceConnectionService deviceConnectionService,
        ILocalizationResourceManager localizationResourceManager)
    {
        ConfigurationsService = configurationsService;
        _navigationService = navigationService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);

        SettingSelection = new SettingSelectionViewModel(configurationsService, deviceConnectionService,
            localizationResourceManager, OpenSettingAsync);
    }

    public ConfigurationsService ConfigurationsService { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    public SettingSelectionViewModel SettingSelection { get; }

    [RelayCommand]
    private Task GoToConfigurationsAsync()
    {
        return _navigationService.NavigateToAsync(Routes.ConfigurationList);
    }

    private Task OpenSettingAsync(Dac70SettingValue value)
    {
        if (ConfigurationsService.SelectedConfiguration is not { } configuration)
        {
            return Task.CompletedTask;
        }

        return _navigationService.NavigateToAsync(Routes.EditSetting, new Dictionary<string, object>
        {
            { EditSettingNavigationParameters.Configuration, configuration },
            { EditSettingNavigationParameters.SettingValue, value }
        });
    }
}
