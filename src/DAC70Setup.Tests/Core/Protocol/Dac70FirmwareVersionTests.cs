using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Tests.Core.Protocol;

public class Dac70FirmwareVersionTests
{
    // The version is a little-endian 16-bit number in bytes 2-3, one hex digit per part
    [Fact]
    public void FromReadResponse_SplitsLittleEndianWordIntoNibbles()
    {
        var version = Dac70FirmwareVersion.FromReadResponse(Report.Create(0x13, 0x3E, 0x34, 0x12));

        Assert.Equal(new Dac70FirmwareVersion(1, 2, 3, 4), version);
    }

    [Fact]
    public void ToString_WritesPartsAsHexDigits()
    {
        var version = Dac70FirmwareVersion.FromReadResponse(Report.Create(0x13, 0x3E, 0xCD, 0xAB));

        Assert.Equal("a.b.c.d", version?.ToString());
    }

    [Fact]
    public void FromReadResponse_WrongHeader_ReturnsNull()
    {
        Assert.Null(Dac70FirmwareVersion.FromReadResponse(Report.Create(0x00, 0x3E, 0x34, 0x12)));
    }

    [Fact]
    public void FromReadResponse_WrongLength_ReturnsNull()
    {
        Assert.Null(Dac70FirmwareVersion.FromReadResponse([0x13, 0x3E, 0x34, 0x12]));
    }
}