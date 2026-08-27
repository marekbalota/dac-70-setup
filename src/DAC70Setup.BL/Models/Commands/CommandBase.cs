using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DAC70Setup.BL.Models.Commands;

public abstract class CommandBase: DelegateCommandBase
{
    private bool _destroyed;

    public bool Initialized { get; private set; }

    public void Initialize(INotifyPropertyChanged model)
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
        ViewModel = model;
        OnViewModelSet();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the field as nullable.
    protected INotifyPropertyChanged ViewModel { get; private set; }
#pragma warning restore CS8618

    protected virtual void OnViewModelSet()
    {
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Counterpart of <see cref="OnViewModelSet"/>, invoked only when a view model was actually set.
    /// Everything tied to the view model belongs here rather than in <see cref="OnDestroy"/>.
    /// </summary>
    protected virtual void OnViewModelUnset()
    {
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    protected virtual void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    protected void ObserveProperty<T>(Expression<Func<T>> propertyExpression)
    {
        for (Expression? node = propertyExpression.Body; node is MemberExpression member; node = member.Expression)
        {
            if (member.Member is not PropertyInfo)
            {
                throw new ArgumentException(
                    $"'{member.Member.Name}' is a field and cannot be observed, because change notification is " +
                    "raised for properties only. Expose it as a property instead.",
                    nameof(propertyExpression));
            }
        }

        ObservesPropertyInternal(propertyExpression);
    }

    /// <summary>
    /// Invoked whether or not the command was initialized, for anything it owns from construction.
    /// </summary>
    protected virtual void OnDestroy()
    {
    }

    public void Destroy()
    {
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;

        if (Initialized)
        {
            OnViewModelUnset();
        }

        OnDestroy();
    }
}
