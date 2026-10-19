using CommunityToolkit.Maui.Views;

namespace DAC70Setup.BL.Features.Configurations.Popups.RenameConfiguration;

public partial class RenameConfigurationPopup : Popup<string>
{
    public RenameConfigurationPopup(RenameConfigurationPopupViewModel viewModel)
    {
        BindingContext = viewModel;

        InitializeComponent();
    }
}
