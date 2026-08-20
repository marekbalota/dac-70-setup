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
    }

    public DataModel Model { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }
}
