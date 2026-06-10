using DAC70Setup.BL.Models.Commands;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class SelectConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    public SelectConfigurationCommand()
    {
        ObserveProperties(nameof(ViewModel.IsBusy), nameof(ViewModel.SelectedConfiguration));
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy && ViewModel.SelectedConfiguration != null;
    }

    protected override void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
