using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.ConfigurationsLanding;
using DAC70Setup.BL.Features.EditConfiguration;
using DAC70Setup.BL.Features.Main;
using DAC70Setup.BL.Features.SelectedConfiguration;
using DAC70Setup.BL.Features.Settings;
using Prism.Navigation.Builder;

namespace DAC70Setup.BL.Services.Navigation;

public class NavigationUri : INavigationUri
{
    private readonly INavigationService _navigationService;
    public NavigationUri(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public INavigationBuilder CreateConfigurationsNavigationBuilder()
    {
        return _navigationService.CreateBuilder()
            .AddSegment(nameof(MainPage))
            .AddNavigationPage()
            .AddTabbedSegment(nameof(ConfigurationsLandingPage), config =>
            {
                config.CreateTab<SelectedConfigurationPage>();
                config.CreateTab<EditConfigurationPage>();
                config.CreateTab<ConfigurationsPage>();
            });
    }

    public INavigationBuilder CreateSettingsNavigationBuilder()
    {
        return _navigationService.CreateBuilder()
            .AddSegment(nameof(MainPage))
            .AddNavigationPage()
            .AddSegment(nameof(SettingsPage));
    }
}
