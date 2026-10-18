namespace DAC70Setup.BL.Features.SelectedConfiguration;

public partial class SelectedConfigurationPage : ContentPage
{
    public SelectedConfigurationPage(SelectedConfigurationPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
