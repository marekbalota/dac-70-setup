using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Tests.Core.Definitions;

public class Dac70SettingDefinitionTests
{
    // The read request sets bit 0x20 in the type and moves the setting id to byte 4
    [Theory]
    [InlineData("Input Source", 0xE0, 0x04)]
    [InlineData("Output Format", 0xE0, 0x02)]
    [InlineData("OSD Hide Mode", 0xE0, 0x15)]
    public void GetReadPayload_C0Setting_RequestsItsOwnId(string settingName, int readType, int requestId)
    {
        byte[] payload = Catalog.Get<Dac70SettingDefinition>(settingName).GetReadPayload();

        Assert.Equal(Report.Create(0x13, 0x3E, (byte)readType, 0x00, (byte)requestId), payload);
    }

    // One c1 reply carries all four Proc Amp values, so they share request id 0x02
    [Theory]
    [InlineData("Brightness")]
    [InlineData("Contrast")]
    [InlineData("Hue")]
    [InlineData("Saturation")]
    public void GetReadPayload_RangeSetting_SharesRequestId(string settingName)
    {
        byte[] payload = Catalog.Get<Dac70SettingDefinition>(settingName).GetReadPayload();

        Assert.Equal(Report.Create(0x13, 0x3E, 0xE1, 0x00, 0x02), payload);
    }

    [Fact]
    public void CreateValueFromReadResponse_C0Setting_ReadsOptionFromByte2()
    {
        var definition = Catalog.Get<EnumSettingDefinition>("Input Source");

        var value = definition.CreateValueFromReadResponse(Report.Create(0x13, 0x3E, 0x04));

        Assert.Equal("SDI", Assert.IsType<EnumSettingValue>(value).SelectedOption.Name);
    }

    [Theory]
    [InlineData("Brightness", 0x11)]
    [InlineData("Contrast", 0x22)]
    [InlineData("Hue", 0x33)]
    [InlineData("Saturation", 0x44)]
    public void CreateValueFromReadResponse_RangeSetting_ReadsByteAtItsId(string settingName, int expected)
    {
        var definition = Catalog.Get<RangeSettingDefinition>(settingName);
        byte[] response = Report.Create(0x13, 0x3E, 0x11, 0x22, 0x33, 0x44);

        var value = definition.CreateValueFromReadResponse(response);

        Assert.Equal(expected, Assert.IsType<RangeSettingValue>(value).Value);
    }

    [Fact]
    public void CreateValueFromReadResponse_UnreadableSetting_ReturnsNull()
    {
        var definition = Catalog.Get<Dac70SettingDefinition>("No Signal Mode");

        Assert.Null(definition.CreateValueFromReadResponse(Report.Create(0x13, 0x3E, 0x01)));
    }

    [Fact]
    public void CreateValueFromReadResponse_WrongHeader_ReturnsNull()
    {
        var definition = Catalog.Get<EnumSettingDefinition>("Input Source");

        Assert.Null(definition.CreateValueFromReadResponse(Report.Create(0x13, 0x3F, 0x04)));
    }

    [Fact]
    public void CreateValueFromReadResponse_ShortReport_ReturnsNull()
    {
        var definition = Catalog.Get<EnumSettingDefinition>("Input Source");

        Assert.Null(definition.CreateValueFromReadResponse([0x13, 0x3E, 0x04]));
    }

    [Fact]
    public void CreateValueFromReadResponse_UnknownOption_ReturnsNull()
    {
        var definition = Catalog.Get<EnumSettingDefinition>("Input Source");

        Assert.Null(definition.CreateValueFromReadResponse(Report.Create(0x13, 0x3E, 0x03)));
    }
}