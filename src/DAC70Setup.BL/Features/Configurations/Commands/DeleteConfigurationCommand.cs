using System.ComponentModel;
using DAC70Setup.BL.Models.Commands;

namespace DAC70Setup.BL.Features.Configurations.Commands;

public class DeleteConfigurationCommand : ActiveCommandBase<ConfigurationsPageViewModel>
{
    public DeleteConfigurationCommand()
    {
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsBusy) || e.PropertyName == nameof(ViewModel.SelectedConfiguration))
        {
            RaiseCanExecuteChanged();
        }
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

        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}
