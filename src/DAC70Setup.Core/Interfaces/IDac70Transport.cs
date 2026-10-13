using DAC70Setup.Core.Models;

namespace DAC70Setup.Core.Interfaces;

public interface IDac70Transport
{
    string Id { get; }

    string DisplayName { get; }

    bool IsSupported { get; }

    event EventHandler? DevicesChanged;

    IAsyncEnumerable<Dac70Device> DiscoverAsync(CancellationToken cancellationToken);

    Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken);
}
