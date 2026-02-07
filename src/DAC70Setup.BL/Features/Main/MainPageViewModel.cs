using DAC70Setup.BL.Features.Main.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Resources;
using DAC70Setup.BL.Services.Navigation;
using DAC70Setup.BL.ViewModels;

namespace DAC70Setup.BL.Features.Main;

public class MainPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly INavigationUri _navigationUri;

    public MainPageViewModel(INavigationService navigationService, INavigationUri navigationUri)
    {
        _navigationService = navigationService;
        _navigationUri = navigationUri;

        ItemSelectedCommand = new DelegateCommand(async () =>
        {
            await _navigationService.NavigateAsync(SelectedItem.NavigateTo);
        });

        NavigationItems = GetNavigationItems().ToArray();
    }

    private FlyoutNavigationItem[] _navigationItems;
    public FlyoutNavigationItem[] NavigationItems
    {
        get => _navigationItems;
        set => SetProperty(ref _navigationItems, value);
    }

    private FlyoutNavigationItem _selectedItem;
    public FlyoutNavigationItem SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public DelegateCommand ItemSelectedCommand { get; }

    private IEnumerable<FlyoutNavigationItem> GetNavigationItems()
    {
        yield return new FlyoutNavigationItem(nameof(MainPageSR.Configurations), nameof(MainPageSR.ConfigurationsDesc), AwesomeSolidIconFont.Toolbox, _navigationUri.CreateConfigurationsNavigationBuilder().UseAbsoluteNavigation().Uri);
        yield return new FlyoutNavigationItem(nameof(MainPageSR.Settings), nameof(MainPageSR.SettingsDesc), AwesomeSolidIconFont.Gear, _navigationUri.CreateSettingsNavigationBuilder().UseAbsoluteNavigation().Uri);
    }
}
