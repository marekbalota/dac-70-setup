using System.Windows.Input;

namespace DAC70Setup.BL.Controls;

public partial class NoConfigurationSelectedView : ContentView
{
    public static readonly BindableProperty GoToConfigurationsCommandProperty =
        BindableProperty.Create(nameof(GoToConfigurationsCommand), typeof(ICommand), typeof(NoConfigurationSelectedView));

    public NoConfigurationSelectedView()
    {
        InitializeComponent();
    }

    public ICommand? GoToConfigurationsCommand
    {
        get => (ICommand?)GetValue(GoToConfigurationsCommandProperty);
        set => SetValue(GoToConfigurationsCommandProperty, value);
    }
}
