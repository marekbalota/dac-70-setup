using CommunityToolkit.Maui.Views;

namespace DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration;

public partial class DeleteConfigurationPopup : Popup<bool>
{
    public DeleteConfigurationPopup(DeleteConfigurationPopupViewModel viewModel)
    {
        BindingContext = viewModel;

        InitializeComponent();
    }
}
