using CommunityToolkit.Maui;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Features.ConfigurationsLanding;
using DAC70Setup.BL.Features.ConfigurationsLanding.Resources;
using DAC70Setup.BL.Features.EditConfiguration;
using DAC70Setup.BL.Features.EditConfiguration.Resources;
using DAC70Setup.BL.Features.Main;
using DAC70Setup.BL.Features.Main.Resources;
using DAC70Setup.BL.Features.SelectedConfiguration;
using DAC70Setup.BL.Features.SelectedConfiguration.Resources;
using DAC70Setup.BL.Features.Settings;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Resources;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Commands;
using DAC70Setup.BL.Services.Navigation;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Data;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Logging;

namespace DAC70Setup;

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
                localization.AddResource(SelectedConfigurationPageSR.ResourceManager);
                localization.AddResource(EditConfigurationPageSR.ResourceManager);
                localization.AddResource(ConfigurationsPageSR.ResourceManager);
                localization.RestoreLatestCulture(true);
            })
            .UsePrism(prism =>
            {
                prism.RegisterTypes(RegisterTypes);
                prism.OnInitialized(InitializeServices);
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

        string databasePath = Path.Combine(FileSystem.AppDataDirectory, "dac70setup.db3");
        containerRegistry.RegisterInstance<IConfigurationRepository>(new ConfigurationRepository(databasePath));
        containerRegistry.RegisterSingleton<IConfigurationsService, ConfigurationsService>();

        containerRegistry.RegisterSingleton<INavigationUri, NavigationUri>();
        containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
        containerRegistry.RegisterSingleton<DataModel>();
    }

    private static void RegisterNavigation(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        containerRegistry.RegisterForNavigation<ConfigurationsLandingPage, ConfigurationsLandingPageViewModel>();
        containerRegistry.RegisterForNavigation<SelectedConfigurationPage, SelectedConfigurationPageViewModel>();
        containerRegistry.RegisterForNavigation<EditConfigurationPage, EditConfigurationPageViewModel>();
        containerRegistry.RegisterForNavigation<ConfigurationsPage, ConfigurationsPageViewModel>();
        containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
    }

    private static void InitializeServices(IContainerProvider containerProvider)
    {
        var configurationsService = containerProvider.Resolve<IConfigurationsService>();

        // Prism invokes this on the UI thread and then blocks that thread on the CreateWindow task
        // (onStart.Wait()), so awaiting here would deadlock.
        Task.Run(configurationsService.InitializeAsync).GetAwaiter().GetResult();
    }

    private static Task InitialView(IContainerProvider containerProvider, INavigationService navigationService)
    {
        var navigationUri = containerProvider.Resolve<INavigationUri>();

        return navigationService.NavigateAsync(navigationUri.CreateConfigurationsNavigationBuilder().UseAbsoluteNavigation().Uri);
    }
}
