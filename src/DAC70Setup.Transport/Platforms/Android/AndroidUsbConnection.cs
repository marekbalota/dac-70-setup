using Android.Hardware.Usb;
using DAC70Setup.Core.Exceptions;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Transport.Usb;

internal class AndroidUsbConnection : IDac70Connection
{
    private const int ReadTimeoutMilliseconds = 250;
    private const int WriteTimeoutMilliseconds = 1000;

    private readonly UsbDeviceConnection _connection;
    private readonly UsbInterface _usbInterface;
    private readonly UsbEndpoint _outEndpoint;
    private readonly UsbEndpoint _inEndpoint;

    public AndroidUsbConnection(UsbManager manager, UsbDevice device)
    {
        _usbInterface = FindHidInterface(device);
        _outEndpoint = FindEndpoint(_usbInterface, Dac70UsbIdentity.OutEndpointAddress);
        _inEndpoint = FindEndpoint(_usbInterface, Dac70UsbIdentity.InEndpointAddress);

        UsbDeviceConnection? connection;

        try
        {
            connection = manager.OpenDevice(device);
        }
        catch (Java.Lang.SecurityException)
        {
            throw new Dac70PermissionDeniedException();
        }

        if (connection == null)
        {
            throw new IOException("Android could not open the DAC-70.");
        }

        if (!connection.ClaimInterface(_usbInterface, true))
        {
            connection.Close();
            throw new IOException("Android could not claim the DAC-70's USB interface.");
        }

        _connection = connection;
    }

    public Task WriteReportAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken)
    {
        if (report.Length != Dac70UsbIdentity.ReportLength)
        {
            throw new ArgumentException(
                $"Expected a {Dac70UsbIdentity.ReportLength}-byte report, got {report.Length}.", nameof(report));
        }

        var buffer = report.ToArray();

        return Task.Run(() =>
        {
            int written = _connection.BulkTransfer(_outEndpoint, buffer, buffer.Length, WriteTimeoutMilliseconds);

            if (written != buffer.Length)
            {
                throw new IOException("The DAC-70 did not accept the report.");
            }
        }, cancellationToken);
    }

    public Task<byte[]> ReadReportAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var buffer = new byte[Dac70UsbIdentity.ReportLength];

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                long started = Environment.TickCount64;

                int read = _connection.BulkTransfer(_inEndpoint, buffer, buffer.Length, ReadTimeoutMilliseconds);
                if (read > 0)
                {
                    return buffer;
                }

                // -1 means timeout or error and Android does not say which.
                if (read < 0 && Environment.TickCount64 - started < ReadTimeoutMilliseconds / 2)
                {
                    throw new IOException("Reading from the DAC-70 failed.");
                }
            }
        }, cancellationToken);
    }

    private static UsbInterface FindHidInterface(UsbDevice device)
    {
        for (int index = 0; index < device.InterfaceCount; index++)
        {
            var usbInterface = device.GetInterface(index);

            if (usbInterface.InterfaceClass == UsbClass.Hid)
            {
                return usbInterface;
            }
        }

        throw new IOException("The DAC-70 has no HID interface.");
    }

    private static UsbEndpoint FindEndpoint(UsbInterface usbInterface, byte address)
    {
        for (int index = 0; index < usbInterface.EndpointCount; index++)
        {
            var endpoint = usbInterface.GetEndpoint(index);

            if (endpoint != null && (int)endpoint.Address == address)
            {
                return endpoint;
            }
        }

        throw new IOException($"The DAC-70 has no USB endpoint 0x{address:X2}.");
    }

    public ValueTask DisposeAsync()
    {
        _connection.ReleaseInterface(_usbInterface);
        _connection.Close();

        return ValueTask.CompletedTask;
    }
}
