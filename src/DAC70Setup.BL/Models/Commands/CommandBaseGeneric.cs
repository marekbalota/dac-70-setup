using System.ComponentModel;
using DAC70Setup.BL.Models.ViewModels;

namespace DAC70Setup.BL.Models.Commands;

public abstract class CommandBase<TModel> : CommandBase
    where TModel : INotifyPropertyChanged, IBusyAware
{
    protected new TModel ViewModel => (TModel)base.ViewModel;

    protected async Task RunBusyAsync(Func<Task> action)
    {
        ViewModel.IsBusy = true;

        try
        {
            await action();
        }
        finally
        {
            ViewModel.IsBusy = false;
        }
    }
}
