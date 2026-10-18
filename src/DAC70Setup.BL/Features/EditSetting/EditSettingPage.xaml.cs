namespace DAC70Setup.BL.Features.EditSetting;

public partial class EditSettingPage : ContentPage
{
    public EditSettingPage(EditSettingPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
