namespace DAC70Setup.BL.Models.ViewModels;

public abstract class ViewModelBase : BindableBase, INavigationAware, IInitializeAsync, IDestructible, IBusyAware
{
    public ViewModelBase()
    {

    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    protected virtual Task OnNavigatedFromInt(INavigationParameters parameters)
    {
        return Task.CompletedTask;
    }

    public async void OnNavigatedFrom(INavigationParameters parameters)
    {
        await OnNavigatedFromInt(parameters);
    }

    protected virtual Task OnNavigatedToInt(INavigationParameters parameters)
    {
        return Task.CompletedTask;
    }

    public async void OnNavigatedTo(INavigationParameters parameters)
    {
        await OnNavigatedToInt(parameters);
    }

    public Task InitializeAsync(INavigationParameters parameters)
    {
        return InitializeAsyncInt(parameters);
    }

    protected virtual Task InitializeAsyncInt(INavigationParameters parameters)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnDestroy()
    {

    }

    public void Destroy()
    {
        OnDestroy();
    }
}
