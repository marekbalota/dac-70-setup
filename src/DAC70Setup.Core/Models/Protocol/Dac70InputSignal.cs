using System.Globalization;

namespace DAC70Setup.Core.Models.Protocol;

public record Dac70InputSignal(int Width, int Height, double FrameRate, bool IsInterlaced)
{
    public const string Name = "Input Signal";

    /// <summary>
    /// A size of 0x0 means nothing is connected to the device's input.
    /// </summary>
    public bool HasSignal => Width > 0 && Height > 0;

    public static byte[] GetReadPayload()
    {
        var payload = new byte[Dac70UsbIdentity.ReportLength];
        payload[0] = 0x13;
        payload[1] = 0x3E;
        payload[2] = 0x62;
        payload[3] = 0x01;
        payload[5] = 0x14;

        return payload;
    }

    /// <summary>
    /// Bytes 2-3 hold the width and 4-5 the height, both big-endian, byte 6 the whole frame rate,
    /// byte 7 its hundredths, and byte 8 is non-zero for interlaced.
    /// </summary>
    /// <returns>Null when the response is not a DAC-70 reply.</returns>
    public static Dac70InputSignal? FromReadResponse(ReadOnlySpan<byte> response)
    {
        if (response.Length != Dac70UsbIdentity.ReportLength || response[0] != 0x13 || response[1] != 0x3E)
        {
            return null;
        }

        int width = response[2] << 8 | response[3];
        int height = response[4] << 8 | response[5];
        double frameRate = response[6] + response[7] / 100.0;

        return new Dac70InputSignal(width, height, frameRate, response[8] != 0);
    }

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Width}x{Height} {FrameRate:0.00}{(IsInterlaced ? 'i' : 'p')}");
}
