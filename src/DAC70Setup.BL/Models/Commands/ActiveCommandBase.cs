using System.ComponentModel;
using DAC70Setup.BL.Models.ViewModels;

namespace DAC70Setup.BL.Models.Commands;

public abstract class ActiveCommandBase<TModel> : CommandBase<TModel>
    where TModel : INotifyPropertyChanged, IActiveAware, IBusyAware
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

    protected override void OnViewModelUnset()
    {
        base.OnViewModelUnset();

        ViewModel.IsActiveChanged -= OnIsActiveChanged;
    }
}
