using DAC70Setup.BL.Features.SelectedConfiguration;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class SelectConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    private readonly IConfigurationsService _configurationsService;
    private readonly INavigationService _navigationService;

    public SelectConfigurationCommand(IConfigurationsService configurationsService,
        INavigationService navigationService)
    {
        _configurationsService = configurationsService;
        _navigationService = navigationService;
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ObserveProperty(() => ViewModel.IsBusy);
        ObserveProperty(() => ViewModel.SelectedConfiguration);
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy && ViewModel.SelectedConfiguration != null;
    }

    protected override async void Execute(object? parameter)
    {
        await RunBusyAsync(async () =>
        {
            _configurationsService.SelectConfiguration(ViewModel.SelectedConfiguration!);
            await _navigationService.SelectTabAsync(nameof(SelectedConfigurationPage));
        });
    }
}
