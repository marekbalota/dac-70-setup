namespace DAC70Setup.Core.Interfaces;

public interface IDac70Connection : IAsyncDisposable
{
    Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken);

    Task<byte[]> ReadReportAsync(CancellationToken cancellationToken);
}
