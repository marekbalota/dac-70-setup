using DAC70Setup.BL.Features.Main.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Resources;
using DAC70Setup.BL.Services.Navigation;

namespace DAC70Setup.BL.Features.Main;

public class MainPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly INavigationUri _navigationUri;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MainPageViewModel(INavigationService navigationService, INavigationUri navigationUri)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _navigationService = navigationService;
        _navigationUri = navigationUri;

        ItemSelectedCommand = new DelegateCommand(async () =>
        {
            await _navigationService.NavigateAsync(SelectedItem?.NavigateTo);
        });

        NavigationItems = GetNavigationItems().ToArray();
    }

    private FlyoutNavigationItem[] _navigationItems;
    public FlyoutNavigationItem[] NavigationItems
    {
        get => _navigationItems;
        set => SetProperty(ref _navigationItems, value);
    }

    private FlyoutNavigationItem? _selectedItem;
    public FlyoutNavigationItem? SelectedItem
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
