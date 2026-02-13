namespace DAC70Setup.BL.ViewModels;

public class TabViewModelBase : ViewModelBase, IActiveAware
{
    public TabViewModelBase() : base()
    {

    }

    public event EventHandler IsActiveChanged;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value, RaiseIsActiveChanged);
    }

    protected virtual void RaiseIsActiveChanged()
    {
        IsActiveChanged?.Invoke(this, EventArgs.Empty);
    }
}
