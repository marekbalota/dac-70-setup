using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Tests.Core.Values;

public class PositionSettingValueTests
{
    private static PositionSettingDefinition Definition => Catalog.Get<PositionSettingDefinition>("Position");

    // Thesis Table 3.5: 0004 constant, big-endian horizontal in bytes 6-7, vertical in byte 9
    [Theory]
    [InlineData(5, 5, new byte[] { 0x00, 0x05, 0x00, 0x05 })]
    [InlineData(600, 100, new byte[] { 0x02, 0x58, 0x00, 0x64 })]
    [InlineData(300, 50, new byte[] { 0x01, 0x2C, 0x00, 0x32 })]
    public void GetPayload_WritesBigEndianHorizontalAndVertical(int horizontal, int vertical, byte[] expectedValueTail)
    {
        var value = new PositionSettingValue(Definition) { Horizontal = horizontal, Vertical = vertical };

        byte[] payload = value.GetPayload();

        Assert.Equal(Report.Create([0x13, 0x3E, 0x61, 0x11, 0x00, 0x04, .. expectedValueTail]), payload);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(600, 100)]
    [InlineData(256, 77)]
    public void SetValueBytes_BytesFromGetValueBytes_RestoresBothComponents(int horizontal, int vertical)
    {
        var original = new PositionSettingValue(Definition) { Horizontal = horizontal, Vertical = vertical };

        var restored = new PositionSettingValue(Definition);
        restored.SetValueBytes(original.GetValueBytes());

        Assert.Equal((horizontal, vertical), (restored.Horizontal, restored.Vertical));
    }

    [Fact]
    public void Constructor_StartsAtMinimum()
    {
        var value = new PositionSettingValue(Definition);

        Assert.Equal((5, 5), (value.Horizontal, value.Vertical));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(601)]
    public void Horizontal_OutOfRange_Throws(int horizontal)
    {
        var value = new PositionSettingValue(Definition);

        Assert.Throws<ArgumentOutOfRangeException>(() => value.Horizontal = horizontal);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(101)]
    public void Vertical_OutOfRange_Throws(int vertical)
    {
        var value = new PositionSettingValue(Definition);

        Assert.Throws<ArgumentOutOfRangeException>(() => value.Vertical = vertical);
    }

    [Fact]
    public void SetValueBytes_OutOfRangeHorizontal_Throws()
    {
        var value = new PositionSettingValue(Definition);

        Assert.Throws<ArgumentOutOfRangeException>(() => value.SetValueBytes([0x00, 0x04, 0x03, 0xE8, 0x00, 0x05]));
    }
}