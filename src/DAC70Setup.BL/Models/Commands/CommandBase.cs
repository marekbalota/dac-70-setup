using System.ComponentModel;

namespace DAC70Setup.BL.Models.Commands;

public abstract class CommandBase: DelegateCommandBase
{
    private string[]? observedProperties;

    public void Initialize(INotifyPropertyChanged model)
    {
        ViewModel = model;
        OnViewModelSet();

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    protected INotifyPropertyChanged ViewModel { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected virtual void OnViewModelSet()
    {
    }

    protected void ObserveProperties(params string[] names)
    {
        observedProperties = names;
    }

    protected virtual void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (observedProperties?.Contains(e.PropertyName) ?? false)
        {
            RaiseCanExecuteChanged();
        }
    }

    protected virtual void OnDestroy()
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    public void Destroy()
    {
        OnDestroy();
    }
}
