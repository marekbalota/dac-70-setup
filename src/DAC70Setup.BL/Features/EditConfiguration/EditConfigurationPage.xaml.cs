namespace DAC70Setup.BL.Features.EditConfiguration;

public partial class EditConfigurationPage : ContentPage
{
    public EditConfigurationPage(EditConfigurationPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
