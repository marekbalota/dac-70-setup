namespace DAC70Setup.Tests.Fakes;

/// <summary>
/// Queues dispatched work until the test runs it, so a test can tell work sent to the UI thread
/// from work done straight away on the calling thread.
/// </summary>
public class FakeDispatcher : IDispatcher
{
    private readonly Queue<Action> _queued = [];

    public bool IsDispatchRequired => true;

    public bool Dispatch(Action action)
    {
        _queued.Enqueue(action);

        return true;
    }

    public void RunQueued()
    {
        while (_queued.TryDequeue(out var action))
        {
            action();
        }
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