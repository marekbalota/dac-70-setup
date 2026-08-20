using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class DeleteConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    private readonly IConfigurationsService _configurationsService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public DeleteConfigurationCommand(IConfigurationsService configurationsService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _pageDialogService = pageDialogService;
        _localizationResourceManager = localizationResourceManager;

        ObserveProperties(nameof(ViewModel.IsBusy), nameof(ViewModel.SelectedConfiguration));
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy && ViewModel.SelectedConfiguration != null;
    }

    protected override async void Execute(object? parameter)
    {
        await RunBusyAsync(async () =>
        {
            var configuration = ViewModel.SelectedConfiguration!;

            bool confirmed = await _pageDialogService.DisplayAlertAsync(
                _localizationResourceManager[nameof(ConfigurationsPageSR.DeleteConfigurationConfirmTitle)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.DeleteConfigurationConfirmMessage), configuration.Name],
                _localizationResourceManager[nameof(ConfigurationsPageSR.DeleteConfigurationConfirmAccept)],
                _localizationResourceManager[nameof(ConfigurationsPageSR.DeleteConfigurationConfirmCancel)]);

            if (!confirmed)
            {
                return;
            }

            await _configurationsService.DeleteConfiguration(configuration);
        });
    }
}
