namespace DAC70Setup.BL.Features.Configurations;

public partial class ConfigurationsPage : ContentPage
{
    public ConfigurationsPage(ConfigurationsPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
