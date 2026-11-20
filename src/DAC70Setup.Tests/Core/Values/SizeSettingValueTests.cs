using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Tests.Core.Values;

public class SizeSettingValueTests
{
    private static SizeSettingDefinition Definition => Catalog.Get<SizeSettingDefinition>("Size");

    // Thesis Table 3.5: 0002 constant, horizontal in byte 6, vertical in byte 7, then 0000
    [Theory]
    [InlineData(80, 80)]
    [InlineData(100, 100)]
    [InlineData(85, 95)]
    public void GetPayload_WritesHorizontalAndVertical(int horizontal, int vertical)
    {
        var value = new SizeSettingValue(Definition) { Horizontal = horizontal, Vertical = vertical };

        byte[] payload = value.GetPayload();

        Assert.Equal(Report.Create(0x13, 0x3E, 0x61, 0x12, 0x00, 0x02, (byte)horizontal, (byte)vertical, 0x00, 0x00), payload);
    }

    [Theory]
    [InlineData(80, 100)]
    [InlineData(93, 81)]
    public void SetValueBytes_BytesFromGetValueBytes_RestoresBothComponents(int horizontal, int vertical)
    {
        var original = new SizeSettingValue(Definition) { Horizontal = horizontal, Vertical = vertical };

        var restored = new SizeSettingValue(Definition);
        restored.SetValueBytes(original.GetValueBytes());

        Assert.Equal((horizontal, vertical), (restored.Horizontal, restored.Vertical));
    }

    [Fact]
    public void Constructor_StartsAtMaximum()
    {
        var value = new SizeSettingValue(Definition);

        Assert.Equal((100, 100), (value.Horizontal, value.Vertical));
    }

    [Theory]
    [InlineData(79)]
    [InlineData(101)]
    public void Horizontal_OutOfRange_Throws(int horizontal)
    {
        var value = new SizeSettingValue(Definition);

        Assert.Throws<ArgumentOutOfRangeException>(() => value.Horizontal = horizontal);
    }

    [Theory]
    [InlineData(79)]
    [InlineData(101)]
    public void Vertical_OutOfRange_Throws(int vertical)
    {
        var value = new SizeSettingValue(Definition);

        Assert.Throws<ArgumentOutOfRangeException>(() => value.Vertical = vertical);
    }
}