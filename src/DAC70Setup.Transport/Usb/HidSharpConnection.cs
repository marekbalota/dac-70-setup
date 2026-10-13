#if WINDOWS || MACCATALYST
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Protocol;
using HidSharp;

namespace DAC70Setup.Transport.Usb;

internal class HidSharpConnection : IDac70Connection
{
    private const int ReadTimeoutMilliseconds = 250;
    private const int WriteTimeoutMilliseconds = 1000;

    private readonly HidStream _stream;
    private readonly int _outputReportLength;
    private readonly int _inputReportLength;

    public HidSharpConnection(HidStream stream)
    {
        _stream = stream;
        _stream.ReadTimeout = ReadTimeoutMilliseconds;
        _stream.WriteTimeout = WriteTimeoutMilliseconds;
        _outputReportLength = stream.Device.GetMaxOutputReportLength();
        _inputReportLength = stream.Device.GetMaxInputReportLength();
    }

    public Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken)
    {
        if (report.Length != Dac70UsbIdentity.ReportLength)
        {
            throw new ArgumentException(
                $"Expected a {Dac70UsbIdentity.ReportLength}-byte report, got {report.Length}.", nameof(report));
        }

        var buffer = new byte[_outputReportLength];
        report.Span.CopyTo(buffer.AsSpan(1));

        return Task.Run(() =>
        {
            try
            {
                _stream.Write(buffer);
            }
            catch (TimeoutException exception)
            {
                throw new IOException("The DAC-70 did not accept the report in time.", exception);
            }
        }, cancellationToken);
    }

    public Task<byte[]> ReadReportAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var buffer = new byte[_inputReportLength];

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int count = _stream.Read(buffer);
                    var report = new byte[Dac70UsbIdentity.ReportLength];
                    buffer.AsSpan(1, Math.Clamp(count - 1, 0, Dac70UsbIdentity.ReportLength)).CopyTo(report);

                    return report;
                }
                catch (TimeoutException)
                {
                    // Nothing arrived yet
                }
            }
        }, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _stream.Dispose();

        return ValueTask.CompletedTask;
    }
}
#endif
