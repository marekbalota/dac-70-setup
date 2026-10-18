using CommunityToolkit.Maui;
using DAC70Setup.BL;
using DAC70Setup.BL.Controls.DeviceStatus.Resources;
using DAC70Setup.BL.Controls.SettingSelection.Resources;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration.Resources;
using DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration;
using DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration.Resources;
using DAC70Setup.BL.Features.Configurations.Resources;
using DAC70Setup.BL.Features.EditConfiguration;
using DAC70Setup.BL.Features.EditConfiguration.Resources;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Features.EditSetting.Resources;
using DAC70Setup.BL.Features.SelectedConfiguration;
using DAC70Setup.BL.Features.SelectedConfiguration.Resources;
using DAC70Setup.BL.Features.Settings;
using DAC70Setup.BL.Features.Settings.Resources;
using DAC70Setup.BL.Mappers;
using DAC70Setup.BL.Resources;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.Navigation;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Data;
using DAC70Setup.Transport.Usb;
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
                fonts.AddFont("IBMPlexSans-Regular.ttf", "PlexSans");
                fonts.AddFont("IBMPlexSans-Medium.ttf", "PlexSansMedium");
                fonts.AddFont("IBMPlexSans-SemiBold.ttf", "PlexSansSemiBold");
                fonts.AddFont("IBMPlexMono-Regular.ttf", "PlexMono");
                fonts.AddFont("IBMPlexMono-Medium.ttf", "PlexMonoMedium");
            })
            .UseLocalizationResourceManager(localization =>
            {
                localization.AddResource(GeneralSR.ResourceManager);
                localization.AddResource(AppShellSR.ResourceManager);
                localization.AddResource(SelectedConfigurationPageSR.ResourceManager);
                localization.AddResource(DeviceStatusBarSR.ResourceManager);
                localization.AddResource(SettingSelectionViewSR.ResourceManager);
                localization.AddResource(EditConfigurationPageSR.ResourceManager);
                localization.AddResource(ConfigurationsPageSR.ResourceManager);
                localization.AddResource(CreateConfigurationPopupSR.ResourceManager);
                localization.AddResource(DeleteConfigurationPopupSR.ResourceManager);
                localization.AddResource(EditSettingPageSR.ResourceManager);
                localization.AddResource(SettingsPageSR.ResourceManager);
                localization.RestoreLatestCulture(true);
            });

        RegisterServices(builder.Services);
        RegisterViews(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        string databasePath = Path.Combine(FileSystem.AppDataDirectory, "dac70setup.db3");
        services.AddSingleton<IConfigurationRepository>(new ConfigurationRepository(databasePath));

        services.AddSingleton<SettingMapper>();
        services.AddSingleton<ConfigurationMapper>();
        services.AddSingleton<ConfigurationsService>();

        services.AddSingleton<IDac70Transport, UsbTransport>();
        services.AddSingleton<DeviceConnectionService>();

        services.AddSingleton<INavigationService, NavigationService>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddSingleton<AppShell>();

        services.AddSingleton<SelectedConfigurationPage>();
        services.AddSingleton<SelectedConfigurationPageViewModel>();

        services.AddSingleton<EditConfigurationPage>();
        services.AddSingleton<EditConfigurationPageViewModel>();

        services.AddSingleton<ConfigurationsPage>();
        services.AddSingleton<ConfigurationsPageViewModel>();

        services.AddSingleton<SettingsPage>();
        services.AddSingleton<SettingsPageViewModel>();

        services.AddTransientWithShellRoute<EditSettingPage, EditSettingPageViewModel>(Routes.EditSetting);

        services.AddTransientPopup<CreateConfigurationPopup, CreateConfigurationPopupViewModel>();
        services.AddTransientPopup<DeleteConfigurationPopup, DeleteConfigurationPopupViewModel>();
    }
}
