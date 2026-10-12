using System.Runtime.CompilerServices;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;

namespace DAC70Setup.Transport.Usb;

/// <summary>
/// The platform part is merged in at compile time: HidSharp on Windows and Mac Catalyst,
/// the USB host API on Android, nothing on iOS.
/// </summary>
public partial class UsbTransport : IDac70Transport
{
    public UsbTransport()
    {
        StartWatching();
    }

    public string DisplayName => "USB";

    public event EventHandler? DevicesChanged;

    public partial bool IsSupported { get; }

    public async IAsyncEnumerable<Dac70Device> DiscoverAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IReadOnlyList<Dac70Device> devices = await Task.Run(() => GetAttachedDevices(), cancellationToken);

        foreach (var device in devices)
        {
            yield return device;
        }
    }

    public partial Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken);

    private partial IReadOnlyList<Dac70Device> GetAttachedDevices();

    partial void StartWatching();

    private void RaiseDevicesChanged()
    {
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }
}
