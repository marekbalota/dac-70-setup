using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Tests;

public static class Report
{
    /// <summary>
    /// Builds a 64-byte DAC-70 report that starts with the given bytes and is zero-filled after them.
    /// </summary>
    public static byte[] Create(params byte[] leadingBytes)
    {
        var report = new byte[Dac70UsbIdentity.ReportLength];
        leadingBytes.CopyTo(report, 0);

        return report;
    }
}