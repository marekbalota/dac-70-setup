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

        NavigateCommand = new DelegateCommand<FlyoutNavigationItem>(async item =>
        {
            var result = await _navigationService.NavigateAsync(item.NavigateTo);
            if (!result.Success)
            {
                return;
            }

            foreach (var navigationItem in NavigationItems)
            {
                navigationItem.IsCurrent = ReferenceEquals(navigationItem, item);
            }
        });

        NavigationItems = GetNavigationItems().ToArray();
    }

    private FlyoutNavigationItem[] _navigationItems;
    public FlyoutNavigationItem[] NavigationItems
    {
        get => _navigationItems;
        set => SetProperty(ref _navigationItems, value);
    }

    public DelegateCommand<FlyoutNavigationItem> NavigateCommand { get; }

    private IEnumerable<FlyoutNavigationItem> GetNavigationItems()
    {
        yield return new FlyoutNavigationItem(nameof(MainPageSR.Configurations), nameof(MainPageSR.ConfigurationsDesc), AwesomeSolidIconFont.Toolbox, _navigationUri.CreateRelativeConfigurationsNavigationBuilder().Uri, isCurrent: true);
        yield return new FlyoutNavigationItem(nameof(MainPageSR.Settings), nameof(MainPageSR.SettingsDesc), AwesomeSolidIconFont.Gear, _navigationUri.CreateRelativeSettingsNavigationBuilder().Uri);
    }
}
