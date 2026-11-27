using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Transport.Virtual;

internal class VirtualDac70Connection : IDac70Connection
{
    // A HID device sends its input report on every poll; waiting this long keeps a reader from spinning
    private const int ReportIntervalMilliseconds = 10;

    private readonly VirtualDac70 _dac70;

    private bool _isDisposed;

    public VirtualDac70Connection(VirtualDac70 dac70)
    {
        _dac70 = dac70;
    }

    public async Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (report.Length != Dac70UsbIdentity.ReportLength)
        {
            throw new ArgumentException(
                $"Expected a {Dac70UsbIdentity.ReportLength}-byte report, got {report.Length}.", nameof(report));
        }

        await _dac70.ReceiveAsync(report, cancellationToken);
    }

    public async Task<byte[]> ReadReportAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        await Task.Delay(ReportIntervalMilliseconds, cancellationToken);

        return await _dac70.GetLastReplyAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _isDisposed = true;

        return ValueTask.CompletedTask;
    }
}