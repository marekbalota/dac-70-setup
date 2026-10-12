#if WINDOWS || MACCATALYST
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;

namespace DAC70Setup.Transport.Usb;

public partial class UsbTransport
{
    public partial bool IsSupported => false;

    public partial Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken)
    {
        throw new PlatformNotSupportedException("USB access is not available on this platform yet.");
    }

    private partial IReadOnlyList<Dac70Device> GetAttachedDevices()
    {
        return [];
    }
}
#endif
