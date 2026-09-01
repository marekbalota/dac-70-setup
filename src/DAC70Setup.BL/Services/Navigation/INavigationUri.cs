using Prism.Navigation.Builder;

namespace DAC70Setup.BL.Services.Navigation;

public interface INavigationUri
{
    INavigationBuilder CreateRootNavigationBuilder();

    INavigationBuilder CreateRelativeConfigurationsNavigationBuilder();

    INavigationBuilder CreateRelativeSettingsNavigationBuilder();
}
