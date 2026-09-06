using System.Globalization;
using DAC70Setup.BL.Features.Settings.Resources;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Resources;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Settings;

public class SettingsPageViewModel : ViewModelBase
{
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public SettingsPageViewModel(ILocalizationResourceManager localizationResourceManager)
    {
        _localizationResourceManager = localizationResourceManager;

        Themes =
        [
            new ThemeOption(
                AppTheme.Light,
                AwesomeSolidIconFont.Sun,
                new LocalizedString(() => localizationResourceManager[nameof(SettingsPageSR.ThemeLight)])),
            new ThemeOption(
                AppTheme.Dark,
                AwesomeSolidIconFont.Moon,
                new LocalizedString(() => localizationResourceManager[nameof(SettingsPageSR.ThemeDark)])),
            new ThemeOption(
                AppTheme.Unspecified,
                AwesomeSolidIconFont.CircleHalfStroke,
                new LocalizedString(() => localizationResourceManager[nameof(SettingsPageSR.ThemeAuto)]),
                new LocalizedString(() => localizationResourceManager[nameof(SettingsPageSR.ThemeAutoHint)]))
        ];

        _selectedTheme = Themes.FirstOrDefault(option => option.Theme == AppThemePreference.Current)
            ?? Themes[0];
        _selectedTheme.IsSelected = true;

        SelectThemeCommand = new DelegateCommand<ThemeOption>(option => SelectedTheme = option);

        Languages =
        [
            new LanguageOption("English", new CultureInfo("en")),
            new LanguageOption("Čeština", new CultureInfo("cs"))
        ];

        _selectedLanguage = Languages.FirstOrDefault(language => language.Culture.TwoLetterISOLanguageName 
            == _localizationResourceManager.CurrentCulture.TwoLetterISOLanguageName) ?? Languages[0];
    }

    public ThemeOption[] Themes { get; }

    private ThemeOption _selectedTheme;
    public ThemeOption SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (value == null || !SetProperty(ref _selectedTheme, value))
            {
                return;
            }

            foreach (var option in Themes)
            {
                option.IsSelected = option == value;
            }

            AppThemePreference.Current = value.Theme;
        }
    }

    public DelegateCommand<ThemeOption> SelectThemeCommand { get; }

    public LanguageOption[] Languages { get; }

    private LanguageOption _selectedLanguage;
    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value != null && SetProperty(ref _selectedLanguage, value))
            {
                _localizationResourceManager.CurrentCulture = value.Culture;
            }
        }
    }

    public string VersionText => AppInfo.Current.VersionString;

    public string AuthorText => "Marek Balota";

    public string LicensesText => "IBM Plex (OFL 1.1), Font Awesome Free (CC BY 4.0)";
}
