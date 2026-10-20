using CommunityToolkit.Maui.Views;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public partial class CreateConfigurationPopup : Popup<CreateConfigurationResult>
{
    private readonly CreateConfigurationPopupViewModel _viewModel;

    public CreateConfigurationPopup(CreateConfigurationPopupViewModel viewModel)
    {
        _viewModel = viewModel;

        InitializeComponent();

        BindingContext = viewModel;

        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;

        _viewModel.Dispose();
    }
}
