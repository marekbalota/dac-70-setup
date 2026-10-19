namespace DAC70Setup.BL.Controls;

public partial class PageTitleView : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageTitleView), string.Empty);

    public static readonly BindableProperty ConfigurationNameProperty =
        BindableProperty.Create(nameof(ConfigurationName), typeof(string), typeof(PageTitleView));

    public PageTitleView()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? ConfigurationName
    {
        get => (string?)GetValue(ConfigurationNameProperty);
        set => SetValue(ConfigurationNameProperty, value);
    }
}
