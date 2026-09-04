using DAC70Setup.BL.Models.Commands;

namespace DAC70Setup.BL.Features.EditConfiguration.Commands;

public class BeginSelectionCommand : ActiveCommandBase<EditConfigurationPageViewModel>
{
    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ObserveProperty(() => ViewModel.IsBusy);
        ObserveProperty(() => ViewModel.IsSelecting);
        ObserveProperty(() => ViewModel.HasNoSettings);
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy && !ViewModel.IsSelecting && !ViewModel.HasNoSettings;
    }

    protected override void Execute(object? parameter)
    {
        ViewModel.IsSelecting = true;
    }
}
