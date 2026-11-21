namespace DAC70Setup.Tests.Fakes;

/// <summary>
/// Runs dispatched work straight away.
/// </summary>
public class FakeDispatcher : IDispatcher
{
    public bool IsDispatchRequired => false;

    public bool Dispatch(Action action)
    {
        action();

        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        throw new NotSupportedException();
    }

    public IDispatcherTimer CreateTimer()
    {
        throw new NotSupportedException();
    }
}