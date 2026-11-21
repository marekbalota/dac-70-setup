using DAC70Setup.Core.Interfaces;

namespace DAC70Setup.Tests.Fakes;

/// <summary>
/// Behaves like the DAC-70: it repeats its last reply and answers only the requests a test gave it replies for.
/// </summary>
public class FakeDac70Connection : IDac70Connection
{
    private readonly List<(byte[] Request, byte[] Reply)> _answers = [];

    private byte[] _lastReply = Report.Create();
    private byte[]? _pendingReply;
    private int _staleReadsLeft;

    /// <summary>
    /// How many reads still return the previous reply after a request, like a device that is slow to answer.
    /// </summary>
    public int StaleReadsAfterRequest { get; set; }

    public void Answer(byte[] request, byte[] reply)
    {
        _answers.Add((request, reply));
    }

    public Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (_, reply) = _answers.FirstOrDefault(candidate => report.Span.SequenceEqual(candidate.Request));

        if (reply != null)
        {
            _pendingReply = reply;
            _staleReadsLeft = StaleReadsAfterRequest;
        }

        return Task.CompletedTask;
    }

    public async Task<byte[]> ReadReportAsync(CancellationToken cancellationToken)
    {
        // Waits like a real read for the next report and throws when cancelled;
        // without it the session's read loop would spin and never notice its timeout
        await Task.Delay(1, cancellationToken);

        if (_pendingReply != null)
        {
            if (_staleReadsLeft > 0)
            {
                _staleReadsLeft--;
            }
            else
            {
                _lastReply = _pendingReply;
                _pendingReply = null;
            }
        }

        return _lastReply.ToArray();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}