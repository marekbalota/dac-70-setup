using DAC70Setup.Core.Models.Values;
using DAC70Setup.Core.Serialization;

namespace DAC70Setup.Tests.Core.Serialization;

public class YamlConfigurationSerializerTests
{
    private readonly YamlConfigurationSerializer _serializer = new();

    // Exported files are kept and shared, so any change to how they look has to be deliberate
    [Fact]
    public void Write_EverySetting_WritesExpectedFile()
    {
        const string expected = """
            Video:
              Input Source: SDI
              Aspect Ratio: Pan Scan 16:9
              HDMI Input EQ: HIGH
              Output Format: 720x576 50.00i
              HDMI Output Color: RGB 444
              Video Output Range: Full Range
              Edge Enhance: Max
              Enable 3G Level-B: Enable
            Image Adjust:
              Horizontal Position: 300
              Vertical Position: 50
              Horizontal Size: 90
              Vertical Size: 85
            Audio:
              Audio Mute: ON
              Audio Source: L/R Audio
              Audio Level: EBU(Europe)
            Proc Amp:
              Brightness: 202
              Contrast: 203
              Hue: 204
              Saturation: 205
            Setting:
              OSD Hide Mode: Enable
              DipSwitch 8(1) Mode: Pan Scan 16:9
              No Signal Mode: Disable Output

            """;

        string written = ConfigurationFile.Write(_serializer, ConfigurationFile.CreateEverySetting());

        Assert.Equal(expected.ReplaceLineEndings("\n"), written);
    }

    [Fact]
    public void Read_WrittenFile_ReturnsWrittenValues()
    {
        List<Dac70SettingValue> values = ConfigurationFile.CreateEverySetting();

        ConfigurationImport import = ConfigurationFile.Read(_serializer, ConfigurationFile.Write(_serializer, values));

        Assert.Empty(import.Issues);
        Assert.Equal(ConfigurationFile.Describe(values), ConfigurationFile.Describe(import.Values));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Video:\n  Input Source:\n")]
    [InlineData("Video:\n  Input Source: ~\n")]
    public void Read_NothingToImport_ReturnsNoValuesAndNoIssues(string yaml)
    {
        ConfigurationImport import = ConfigurationFile.Read(_serializer, yaml);

        Assert.Empty(import.Values);
        Assert.Empty(import.Issues);
    }

    [Theory]
    [InlineData("Video: [SDI")]
    [InlineData("- Video\n")]
    public void Read_InvalidFile_ThrowsInvalidDataException(string yaml)
    {
        Assert.Throws<InvalidDataException>(() => ConfigurationFile.Read(_serializer, yaml));
    }
}