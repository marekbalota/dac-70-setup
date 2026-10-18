using CommunityToolkit.Mvvm.ComponentModel;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Settings;

public partial class ThemeOption : ObservableObject
{
    public ThemeOption(AppTheme theme, string glyph, LocalizedString name, LocalizedString? hint = null)
    {
        Theme = theme;
        Glyph = glyph;
        Name = name;
        Hint = hint;
    }

    public AppTheme Theme { get; }

    public string Glyph { get; }

    public LocalizedString Name { get; }

    public LocalizedString? Hint { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
