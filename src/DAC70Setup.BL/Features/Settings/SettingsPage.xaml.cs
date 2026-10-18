namespace DAC70Setup.BL.Features.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
