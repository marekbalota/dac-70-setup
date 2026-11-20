using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;

namespace DAC70Setup.Transport.Virtual;

/// <summary>
/// Two always-attached emulated DAC-70s, for trying the app.
/// </summary>
public class VirtualDac70Transport : IDac70Transport
{
    private readonly Dictionary<Dac70Device, VirtualDac70> _devices = new()
    {
        [new Dac70Device("virtual-1")] = new VirtualDac70([0x07, 0x80, 0x04, 0x38, 59, 94, 0x01]), // 1920x1080 59.94i
        [new Dac70Device("virtual-2")] = new VirtualDac70([0x05, 0x00, 0x02, 0xD0, 50, 0, 0x00])   // 1280x720 50.00p
    };

    public string Id => "virtual";

    public string DisplayName => "Virtual";

    public bool IsSupported => true;

    // The virtual devices are never plugged in or out
    public event EventHandler? DevicesChanged
    {
        add { }
        remove { }
    }

    public IAsyncEnumerable<Dac70Device> DiscoverAsync(CancellationToken cancellationToken)
    {
        return _devices.Keys.ToAsyncEnumerable();
    }

    public Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken)
    {
        if (!_devices.TryGetValue(device, out VirtualDac70? virtualDevice))
        {
            throw new IOException("There is no such virtual DAC-70.");
        }

        return Task.FromResult<IDac70Connection>(new VirtualDac70Connection(virtualDevice));
    }
}