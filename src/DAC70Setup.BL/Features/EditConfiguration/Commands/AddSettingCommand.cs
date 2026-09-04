using DAC70Setup.BL.Features.AddSetting;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models.Commands;

namespace DAC70Setup.BL.Features.EditConfiguration.Commands;

public class AddSettingCommand : ActiveCommandBase<EditConfigurationPageViewModel>
{
    private readonly INavigationService _navigationService;

    public AddSettingCommand(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ObserveProperty(() => ViewModel.IsBusy);
        ObserveProperty(() => ViewModel.IsSelecting);
        ObserveProperty(() => ViewModel.Model.SelectedConfiguration);
        ObserveProperty(() => ViewModel.Model.SelectedConfiguration!.HasAvailableSettings);
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy
            && !ViewModel.IsSelecting
            && ViewModel.Model.SelectedConfiguration?.HasAvailableSettings == true;
    }

    protected override async void Execute(object? parameter)
    {
        await RunBusyAsync(async () =>
        {
            var configuration = ViewModel.Model.SelectedConfiguration;
            if (configuration == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { EditSettingNavigationParameters.Configuration, configuration }
            };

            await _navigationService.NavigateAsync(nameof(AddSettingPage), parameters);
        });
    }
}
