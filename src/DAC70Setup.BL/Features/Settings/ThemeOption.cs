using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Settings;

public class ThemeOption : BindableBase
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

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
