using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Tests.Core.Values;

public class RangeSettingValueTests
{
    // Thesis Table 3.3: the Proc Amp settings are c1 with the value 0-255 in byte 4
    [Theory]
    [InlineData("Brightness", 0x02, 0x00)]
    [InlineData("Contrast", 0x03, 0x7F)]
    [InlineData("Hue", 0x04, 0x80)]
    [InlineData("Saturation", 0x05, 0xFF)]
    public void GetPayload_PutsValueInByte4(string settingName, int id, int rawValue)
    {
        var value = new RangeSettingValue(Catalog.Get<RangeSettingDefinition>(settingName)) { Value = (byte)rawValue };

        byte[] payload = value.GetPayload();

        Assert.Equal(Report.Create(0x13, 0x3E, 0xC1, (byte)id, (byte)rawValue), payload);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x80)]
    [InlineData(0xFF)]
    public void SetValueBytes_BytesFromGetValueBytes_RestoresValue(int rawValue)
    {
        var definition = Catalog.Get<RangeSettingDefinition>("Brightness");
        var original = new RangeSettingValue(definition) { Value = (byte)rawValue };

        var restored = new RangeSettingValue(definition);
        restored.SetValueBytes(original.GetValueBytes());

        Assert.Equal(original.Value, restored.Value);
    }

    [Fact]
    public void SetValueBytes_WrongByteCount_Throws()
    {
        var value = new RangeSettingValue(Catalog.Get<RangeSettingDefinition>("Hue"));

        Assert.Throws<ArgumentException>(() => value.SetValueBytes([0x01, 0x02]));
    }
}