using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class CreateConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    private readonly ManageConfigurationsService _manageConfigurationsService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public CreateConfigurationCommand(ManageConfigurationsService manageConfigurationsService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _manageConfigurationsService = manageConfigurationsService;
        _pageDialogService = pageDialogService;
        _localizationResourceManager = localizationResourceManager;

        ObserveProperties(nameof(ViewModel.IsBusy));
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy;
    }

    protected override async void Execute(object? parameter)
    {
        var name = await _pageDialogService.DisplayPromptAsync(null, _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptMessage)],
            _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptAccept)],
            _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptCancel)]);

        if (name == null)
        {
            return;
        }

        await _manageConfigurationsService.CreateNewConfiguration(name);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
