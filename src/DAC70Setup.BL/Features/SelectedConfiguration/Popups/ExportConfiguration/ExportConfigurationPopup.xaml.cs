using CommunityToolkit.Maui.Views;

namespace DAC70Setup.BL.Features.SelectedConfiguration.Popups.ExportConfiguration;

public partial class ExportConfigurationPopup : Popup
{
    public ExportConfigurationPopup(ExportConfigurationPopupViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
