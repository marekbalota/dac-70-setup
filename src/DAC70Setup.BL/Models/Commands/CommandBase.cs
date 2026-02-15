using System.ComponentModel;

namespace DAC70Setup.BL.Models.Commands;

public abstract class CommandBase: DelegateCommandBase
{
    public void Initialize(INotifyPropertyChanged model)
    {
        ViewModel = model;
        OnViewModelSet();
    }

    protected virtual void OnViewModelSet()
    {
    }

    protected INotifyPropertyChanged ViewModel { get; private set; }

    protected virtual void OnDestroy()
    {
    }

    public void Destroy()
    {
        OnDestroy();
    }
}
