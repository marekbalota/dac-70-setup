#if WINDOWS || MACCATALYST
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Protocol;
using HidSharp;

namespace DAC70Setup.Transport.Usb;

public partial class UsbTransport
{
    public partial bool IsSupported => true;

    partial void StartWatching()
    {
        DeviceList.Local.Changed += (_, _) => RaiseDevicesChanged();
    }

    private partial IReadOnlyList<Dac70Device> GetAttachedDevices()
    {
        return DeviceList.Local.GetHidDevices(Dac70UsbIdentity.VendorId, Dac70UsbIdentity.ProductId)
            .Select(hidDevice => new Dac70Device(hidDevice.DevicePath))
            .ToList();
    }

    public partial Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken)
    {
        return Task.Run<IDac70Connection>(() =>
        {
            var hidDevice = DeviceList.Local.GetHidDevices(Dac70UsbIdentity.VendorId, Dac70UsbIdentity.ProductId)
                .FirstOrDefault(candidate => candidate.DevicePath == device.Id);

            if (hidDevice == null)
            {
                throw new IOException("The DAC-70 is no longer connected.");
            }

            if (hidDevice.GetMaxOutputReportLength() < Dac70UsbIdentity.ReportLength + 1)
            {
                throw new IOException("The DAC-70 reports an unexpected report size.");
            }

            if (!hidDevice.TryOpen(out HidStream stream))
            {
                throw new IOException("The DAC-70 could not be opened.");
            }

            return new HidSharpConnection(stream);
        }, cancellationToken);
    }
}
#endif
