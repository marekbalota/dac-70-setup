namespace DAC70Setup.Core.Models.Protocol;

public static class Dac70FactoryReset
{
    public static byte[] GetPayload()
    {
        var payload = new byte[Dac70UsbIdentity.ReportLength];
        payload[0] = 0x13;
        payload[1] = 0x3E;
        payload[2] = 0xC0;
        payload[3] = 0x03;
        payload[4] = 0x01;

        return payload;
    }
}
