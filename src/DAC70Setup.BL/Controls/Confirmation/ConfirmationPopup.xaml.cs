using CommunityToolkit.Maui.Views;

namespace DAC70Setup.BL.Controls.Confirmation;

public partial class ConfirmationPopup : Popup<bool>
{
    public ConfirmationPopup(ConfirmationPopupViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
