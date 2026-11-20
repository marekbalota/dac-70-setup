using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Tests.Core.Values;

public class EnumSettingValueTests
{
    // Thesis Tables 3.3 and 3.4: c0 settings carry the option value in byte 4
    [Theory]
    [InlineData("Input Source", "SDI", 0xC0, 0x04, 0x04)]
    [InlineData("Output Format", "1920x1080 60.00p", 0xC0, 0x02, 0x0F)]
    [InlineData("Audio Source", "L/R Audio", 0xC0, 0x06, 0x00)]
    public void GetPayload_C0Setting_PutsOptionValueInByte4(string settingName, string optionName, int type, int id, int optionValue)
    {
        var definition = Catalog.Get<EnumSettingDefinition>(settingName);
        var value = new EnumSettingValue(definition) { SelectedOption = Catalog.GetOption(definition, optionName) };

        byte[] payload = value.GetPayload();

        Assert.Equal(Report.Create(0x13, 0x3E, (byte)type, (byte)id, (byte)optionValue), payload);
    }

    // Thesis Table 3.5: 61 settings write a 6-byte template with the option value in its third byte
    [Theory]
    [InlineData("Enable 3G Level-B", "Enable", new byte[] { 0x61, 0x19, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00 })]
    [InlineData("DipSwitch 8(1) Mode", "Pan Scan 16:9", new byte[] { 0x61, 0x17, 0x00, 0x02, 0x06, 0x01, 0x00, 0x00 })]
    public void GetPayload_S61Setting_WritesTemplateWithOptionValue(string settingName, string optionName, byte[] expectedAfterHeader)
    {
        var definition = Catalog.Get<EnumSettingDefinition>(settingName);
        var value = new EnumSettingValue(definition) { SelectedOption = Catalog.GetOption(definition, optionName) };

        byte[] payload = value.GetPayload();

        Assert.Equal(Report.Create([0x13, 0x3E, .. expectedAfterHeader]), payload);
    }

    public static TheoryData<string, string> AllOptions()
    {
        var data = new TheoryData<string, string>();

        foreach (var definition in Dac70SettingCatalog.All.OfType<EnumSettingDefinition>())
        {
            foreach (var option in definition.Options)
            {
                data.Add(definition.Name, option.Name);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllOptions))]
    public void SetValueBytes_BytesFromGetValueBytes_SelectsSameOption(string settingName, string optionName)
    {
        var definition = Catalog.Get<EnumSettingDefinition>(settingName);
        var option = Catalog.GetOption(definition, optionName);
        var original = new EnumSettingValue(definition) { SelectedOption = option };

        var restored = new EnumSettingValue(definition);
        restored.SetValueBytes(original.GetValueBytes());

        Assert.Equal(option, restored.SelectedOption);
    }

    [Fact]
    public void SetValueBytes_UnknownOptionValue_Throws()
    {
        var value = new EnumSettingValue(Catalog.Get<EnumSettingDefinition>("Input Source"));

        Assert.Throws<ArgumentException>(() => value.SetValueBytes([0x03]));
    }

    [Fact]
    public void SetValueBytes_WrongByteCount_Throws()
    {
        var value = new EnumSettingValue(Catalog.Get<EnumSettingDefinition>("No Signal Mode"));

        Assert.Throws<ArgumentException>(() => value.SetValueBytes([0x01]));
    }

    [Fact]
    public void Constructor_SelectsFirstOption()
    {
        var definition = Catalog.Get<EnumSettingDefinition>("Audio Source");

        var value = new EnumSettingValue(definition);

        Assert.Equal(definition.Options[0], value.SelectedOption);
    }
}