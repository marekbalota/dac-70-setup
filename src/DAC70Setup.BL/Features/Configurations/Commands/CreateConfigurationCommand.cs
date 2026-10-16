using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class CreateConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    private readonly IConfigurationsService _configurationsService;
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public CreateConfigurationCommand(IConfigurationsService configurationsService,
        DeviceConnectionService deviceConnectionService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _deviceConnectionService = deviceConnectionService;
        _pageDialogService = pageDialogService;
        _localizationResourceManager = localizationResourceManager;
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ObserveProperty(() => ViewModel.IsBusy);
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy;
    }

    protected override async void Execute(object? parameter)
    {
        string? name = null;
        bool isFromDevice = false;

        await RunBusyAsync(async () =>
        {
            name = await _pageDialogService.DisplayPromptAsync(
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationTitle)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationNamePrompt)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationCreate)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationCancel)]);

            if (string.IsNullOrWhiteSpace(name) || _deviceConnectionService.SelectedDevice == null || !_deviceConnectionService.CanSend)
            {
                return;
            }

            string fromDevice = _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationFromDevice)];
            string empty = _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationEmpty)];

            string selection = await _pageDialogService.DisplayActionSheetAsync(
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationStartWith)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.NewConfigurationCancel)],
                null, fromDevice, empty);

            if (selection != fromDevice && selection != empty)
            {
                name = null;
                return;
            }

            isFromDevice = selection == fromDevice;
        });

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        name = name.Trim();

        if (!isFromDevice)
        {
            await _configurationsService.CreateNewConfiguration(name);
            return;
        }

        if (_deviceConnectionService.SelectedDevice?.Snapshot.LastFullSync == null && !await _deviceConnectionService.SyncAsync())
        {
            return;
        }

        if (_deviceConnectionService.SelectedDevice?.Snapshot is not { } snapshot)
        {
            return;
        }

        var values = Dac70SettingCatalog.All
            .Select(definition => snapshot.Values.GetValueOrDefault(definition.Key))
            .OfType<Dac70SettingValue>()
            .ToList();

        await _configurationsService.CreateNewConfiguration(name, values);
    }
}
