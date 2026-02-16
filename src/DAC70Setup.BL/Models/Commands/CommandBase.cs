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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    protected INotifyPropertyChanged ViewModel { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected virtual void OnDestroy()
    {
    }

    public void Destroy()
    {
        OnDestroy();
    }
}
