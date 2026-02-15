using System.ComponentModel;

namespace DAC70Setup.BL.Models.Commands;

public abstract class ActiveCommandBase<TModel> : CommandBase<TModel> 
    where TModel : INotifyPropertyChanged, IActiveAware
{
    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ViewModel.IsActiveChanged += OnIsActiveChanged;
        IsActive = ViewModel.IsActive;
    }

    private void OnIsActiveChanged(object? sender, EventArgs e)
    {
        IsActive = ViewModel.IsActive;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ViewModel.IsActiveChanged -= OnIsActiveChanged;
    }
}
