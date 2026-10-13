using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using DAC70Setup.Core.Exceptions;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Transport.Usb;

public partial class UsbTransport
{
    public partial bool IsSupported => ApplicationContext.PackageManager?.HasSystemFeature(PackageManager.FeatureUsbHost) == true;

    private static Context ApplicationContext => Android.App.Application.Context;

    private static UsbManager Manager => ApplicationContext.GetSystemService(Context.UsbService) as UsbManager
        ?? throw new IOException("This Android device has no USB host support.");

    partial void StartWatching()
    {
        var filter = new IntentFilter();
        filter.AddAction(UsbManager.ActionUsbDeviceAttached);
        filter.AddAction(UsbManager.ActionUsbDeviceDetached);

        ApplicationContext.RegisterReceiver(new UsbBroadcastReceiver(_ => RaiseDevicesChanged()), filter);
    }

    private partial IReadOnlyList<Dac70Device> GetAttachedDevices()
    {
        return FindUsbDevices()
            .Select(usbDevice => new Dac70Device(usbDevice.DeviceName, ReadSerialNumber(usbDevice)))
            .ToList();
    }

    private static string? ReadSerialNumber(UsbDevice usbDevice)
    {
        try
        {
            string? serialNumber = usbDevice.SerialNumber;
            return string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber;
        }
        catch (Java.Lang.SecurityException)
        {
            return null;
        }
    }

    public partial async Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken)
    {
        var usbDevice = FindUsbDevices().FirstOrDefault(candidate => candidate.DeviceName == device.Id);

        if (usbDevice == null)
        {
            throw new IOException("The DAC-70 is no longer connected.");
        }

        bool hadPermission = Manager.HasPermission(usbDevice);

        if (!await UsbPermissionRequest.RequestAsync(ApplicationContext, Manager, usbDevice, cancellationToken))
        {
            throw new Dac70PermissionDeniedException();
        }

        if (!hadPermission)
        {
            // Android shows the serial number only after permission is granted.
            RaiseDevicesChanged();
        }

        return new AndroidUsbConnection(Manager, usbDevice);
    }

    private static IEnumerable<UsbDevice> FindUsbDevices()
    {
        IEnumerable<UsbDevice> attached = Manager.DeviceList?.Values ?? [];

        return attached.Where(usbDevice => usbDevice.VendorId == Dac70UsbIdentity.VendorId
            && usbDevice.ProductId == Dac70UsbIdentity.ProductId);
    }
}
