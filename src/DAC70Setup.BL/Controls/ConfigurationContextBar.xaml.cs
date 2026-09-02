namespace DAC70Setup.BL.Controls;

public partial class ConfigurationContextBar : ContentView
{
    public static readonly BindableProperty ConfigurationNameProperty =
        BindableProperty.Create(nameof(ConfigurationName), typeof(string), typeof(ConfigurationContextBar), string.Empty);

    public ConfigurationContextBar()
    {
        InitializeComponent();
    }

    public string ConfigurationName
    {
        get => (string)GetValue(ConfigurationNameProperty);
        set => SetValue(ConfigurationNameProperty, value);
    }
}
