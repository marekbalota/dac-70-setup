namespace DAC70Setup.BL.Features.Configurations;

public partial class ConfigurationsPage : ContentPage
{
    public ConfigurationsPage(ConfigurationsPageViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        BindingContext = viewModel;
    }

    public ConfigurationsPageViewModel ViewModel { get; }
}
