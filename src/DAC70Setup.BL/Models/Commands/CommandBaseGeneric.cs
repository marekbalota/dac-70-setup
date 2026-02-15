using System.ComponentModel;

namespace DAC70Setup.BL.Models.Commands;

public abstract class CommandBase<TModel> : CommandBase 
    where TModel : INotifyPropertyChanged
{
    protected new TModel ViewModel => (TModel)base.ViewModel;
}
