using CommunityToolkit.Maui;
using DAC70Setup.BL.Features.ConfigurationsLanding;
using DAC70Setup.BL.Features.ConfigurationsLanding.Resources;
using DAC70Setup.BL.Features.Main;
using DAC70Setup.BL.Features.Main.Resources;
using DAC70Setup.BL.Features.Settings;
using DAC70Setup.BL.Resources;
using DAC70Setup.BL.Services.Commands;
using DAC70Setup.BL.Services.Navigation;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Logging;

namespace DAC70Setup
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Font_Awesome_7_Free-Solid-900.otf", "FA-S");
                })
                .UseLocalizationResourceManager(localization =>
                {
                    localization.AddResource(GeneralSR.ResourceManager);
                    localization.AddResource(MainPageSR.ResourceManager);
                    localization.AddResource(ConfigurationsLandingPageSR.ResourceManager);
                    localization.RestoreLatestCulture(true);
                })
                .UsePrism(prism =>
                {
                    prism.RegisterTypes(RegisterTypes);
                    prism.CreateWindow(InitialView);
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterNavigation(containerRegistry);

            containerRegistry.RegisterSingleton<INavigationUri, NavigationUri>();
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
        }

        private static void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<ConfigurationsLandingPage, ConfigurationsLandingPageViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
        }

        private static async Task InitialView(IContainerProvider containerProvider, INavigationService navigationService)
        {
            var navigationUri = containerProvider.Resolve<INavigationUri>();

            await navigationService.NavigateAsync(navigationUri.CreateConfigurationsNavigationBuilder().UseAbsoluteNavigation().Uri);
        }
    }
}
