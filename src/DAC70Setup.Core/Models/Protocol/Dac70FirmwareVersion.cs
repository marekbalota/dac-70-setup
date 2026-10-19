namespace DAC70Setup.Core.Models.Protocol;

public record Dac70FirmwareVersion(int Major, int Minor, int Build, int Revision)
{
    public const string Name = "Firmware";

    public static byte[] GetReadPayload()
    {
        var payload = new byte[Dac70UsbIdentity.ReportLength];
        payload[0] = 0x13;
        payload[1] = 0x3E;
        payload[2] = 0xE0;
        payload[4] = 0x05;

        return payload;
    }

    public static Dac70FirmwareVersion? FromReadResponse(ReadOnlySpan<byte> response)
    {
        if (response.Length != Dac70UsbIdentity.ReportLength || response[0] != 0x13 || response[1] != 0x3E)
        {
            return null;
        }

        int version = response[3] << 8 | response[2];

        return new Dac70FirmwareVersion((version >> 12) & 0xF, (version >> 8) & 0xF, (version >> 4) & 0xF, version & 0xF);
    }

    public override string ToString() => $"{Major:x}.{Minor:x}.{Build:x}.{Revision:x}";
}
