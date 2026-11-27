namespace DAC70Setup.Core.Interfaces;

/// <summary>
/// An open connection to one device. Reports are <see cref="Models.Protocol.Dac70UsbIdentity.ReportLength"/> bytes,
/// starting with the 13 3E header.
/// </summary>
public interface IDac70Connection : IAsyncDisposable
{
    /// <exception cref="ArgumentException">The report is not <see cref="Models.Protocol.Dac70UsbIdentity.ReportLength"/> bytes.</exception>
    /// <exception cref="IOException">The device did not accept the report.</exception>
    Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the next report. The device sends its last reply again on every poll,
    /// so this returns the same reply until the device answers a new request.
    /// </summary>
    /// <exception cref="OperationCanceledException">The token was cancelled; the read session's reply timeout relies on this.</exception>
    /// <exception cref="IOException">Reading from the device failed.</exception>
    Task<byte[]> ReadReportAsync(CancellationToken cancellationToken);
}
