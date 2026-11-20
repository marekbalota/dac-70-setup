using DAC70Setup.Core.Models.Values;
using DAC70Setup.Core.Serialization;

namespace DAC70Setup.Tests.Core.Serialization;

// Matching and validating what was read happens in the base class, so YAML here is only the input format
public class ConfigurationSerializerTests
{
    private readonly YamlConfigurationSerializer _serializer = new();

    [Fact]
    public void Read_UnknownPanelOrSetting_ReportsItAndKeepsTheRest()
    {
        const string yaml = """
            Video:
              Input Source: SDI
              Sharpness: 5
            Gamma:
              Level: 2
            """;

        ConfigurationImport import = ConfigurationFile.Read(_serializer, yaml);

        ConfigurationImportIssue[] expectedIssues =
        [
            new(ConfigurationImportIssueKind.UnknownSetting, "Video", "Sharpness", "5"),
            new(ConfigurationImportIssueKind.UnknownSetting, "Gamma", "Level", "2")
        ];
        Assert.Equal(expectedIssues, import.Issues);
        Assert.Equal("SDI", Assert.Single(import.Values).DisplayValueText);
    }

    [Theory]
    [InlineData("Video", "Input Source", "DVI")]
    [InlineData("Proc Amp", "Contrast", "high")]
    [InlineData("Proc Amp", "Brightness", "256")]
    [InlineData("Image Adjust", "Horizontal Position", "4")]
    [InlineData("Image Adjust", "Vertical Position", "101")]
    public void Read_InvalidValue_ReportsIt(string panel, string key, string value)
    {
        ConfigurationImport import = ConfigurationFile.Read(_serializer, $"{panel}:\n  {key}: {value}\n");

        Assert.Equal(new ConfigurationImportIssue(ConfigurationImportIssueKind.InvalidValue, panel, key, value), Assert.Single(import.Issues));
        Assert.Empty(import.Values);
    }

    [Fact]
    public void Read_DifferentCase_StillMatches()
    {
        ConfigurationImport import = ConfigurationFile.Read(_serializer, "video:\n  input source: sdi\n");

        Assert.Empty(import.Issues);
        Assert.Equal("SDI", Assert.Single(import.Values).DisplayValueText);
    }

    [Fact]
    public void Read_OnlyOneHalfOfPosition_FillsTheOtherAndReportsIt()
    {
        ConfigurationImport import = ConfigurationFile.Read(_serializer, "Image Adjust:\n  Horizontal Position: 300\n");

        var position = Assert.IsType<PositionSettingValue>(Assert.Single(import.Values));
        Assert.Equal(300, position.Horizontal);
        Assert.Equal(new ConfigurationImportIssue(ConfigurationImportIssueKind.MissingValueFilled, "Image Adjust", "Vertical Position", "5"),
            Assert.Single(import.Issues));
    }
}