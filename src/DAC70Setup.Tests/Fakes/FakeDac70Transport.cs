using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;

namespace DAC70Setup.Tests.Fakes;

/// <summary>
/// A transport that is used for tests; each device is a <see cref="FakeDac70Connection"/>.
/// </summary>
public class FakeDac70Transport : IDac70Transport
{
    private readonly Dictionary<string, FakeDac70Connection> _devices = [];

    public FakeDac70Transport(string id = "fake")
    {
        Id = id;
    }

    public string Id { get; }

    public string DisplayName => Id;

    public bool IsSupported { get; init; } = true;

    /// <summary>
    /// Thrown by every connect while set, like a device that cannot be opened.
    /// </summary>
    public Exception? ConnectException { get; set; }

    public event EventHandler? DevicesChanged;

    public FakeDac70Connection Attach(string deviceId)
    {
        var connection = new FakeDac70Connection();
        _devices[deviceId] = connection;

        return connection;
    }

    public void Detach(string deviceId)
    {
        _devices.Remove(deviceId);
    }

    public void RaiseDevicesChanged()
    {
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    public IAsyncEnumerable<Dac70Device> DiscoverAsync(CancellationToken cancellationToken)
    {
        return _devices.Keys.Select(id => new Dac70Device(id)).ToList().ToAsyncEnumerable();
    }

    public Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken)
    {
        if (ConnectException != null)
        {
            return Task.FromException<IDac70Connection>(ConnectException);
        }

        if (!_devices.TryGetValue(device.Id, out FakeDac70Connection? connection))
        {
            return Task.FromException<IDac70Connection>(new IOException("The DAC-70 is no longer connected."));
        }

        return Task.FromResult<IDac70Connection>(connection);
    }
}