namespace DAC70Setup.BL.Controls;

public class DescribedFlyoutItem : FlyoutItem
{
    public static readonly BindableProperty IconGlyphProperty =
        BindableProperty.Create(nameof(IconGlyph), typeof(string), typeof(DescribedFlyoutItem), string.Empty);

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(DescribedFlyoutItem), string.Empty);

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
