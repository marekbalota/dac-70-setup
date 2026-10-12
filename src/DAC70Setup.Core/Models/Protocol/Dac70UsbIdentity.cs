namespace DAC70Setup.Core.Models.Protocol;

public static class Dac70UsbIdentity
{
    public const int VendorId = 0x1164;

    public const int ProductId = 0xB246;

    public const int ReportLength = 64;

    public const byte OutEndpointAddress = 0x02;

    public const byte InEndpointAddress = 0x81;
}
