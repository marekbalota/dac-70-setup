using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class CreateConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    private readonly IConfigurationsService _configurationsService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public CreateConfigurationCommand(IConfigurationsService configurationsService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
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
        await RunBusyAsync(async () =>
        {
            var name = await _pageDialogService.DisplayPromptAsync(
                _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptTitle)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptMessage)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptAccept)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.CreateConfigurationDisplayPromptCancel)]);

            if (name == null)
            {
                return;
            }

            await _configurationsService.CreateNewConfiguration(name);
        });
    }
}
