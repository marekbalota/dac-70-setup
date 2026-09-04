namespace DAC70Setup.BL.Controls;

public partial class SettingRow : ContentView
{
    public static readonly BindableProperty NameProperty =
        BindableProperty.Create(nameof(Name), typeof(string), typeof(SettingRow), string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(SettingRow), string.Empty);

    public static readonly BindableProperty ShowChevronProperty =
        BindableProperty.Create(nameof(ShowChevron), typeof(bool), typeof(SettingRow), false,
            propertyChanged: OnShowChevronChanged);

    public static readonly BindableProperty IsSelectingProperty =
        BindableProperty.Create(nameof(IsSelecting), typeof(bool), typeof(SettingRow), false,
            propertyChanged: OnIsSelectingChanged);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(SettingRow), false);

    public SettingRow()
    {
        InitializeComponent();
    }

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsSelecting
    {
        get => (bool)GetValue(IsSelectingProperty);
        set => SetValue(IsSelectingProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool ShowChevron
    {
        get => (bool)GetValue(ShowChevronProperty);
        set => SetValue(ShowChevronProperty, value);
    }

    public bool HasChevron => ShowChevron && !IsSelecting;

    private static void OnShowChevronChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SettingRow row)
        {
            row.OnPropertyChanged(nameof(row.HasChevron));
        }
    }

    private static void OnIsSelectingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SettingRow row)
        {
            row.OnPropertyChanged(nameof(row.HasChevron));
        }
    }
}
