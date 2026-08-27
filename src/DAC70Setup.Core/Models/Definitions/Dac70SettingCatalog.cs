using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Models.Definitions;

public static class Dac70SettingCatalog
{
    public static IReadOnlyList<Dac70SettingDefinition> All { get; } =
    [
        // Video panel, c0 type
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x04), PanelType.Video, "Input Source",
        [
            new EnumOption("VGA", 0x02),
            new EnumOption("HDMI", 0x01),
            new EnumOption("SDI", 0x04)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x07), PanelType.Video, "Aspect Ratio",
        [
            new EnumOption("Keep Aspect Ratio", 0x00),
            new EnumOption("Full Screen (16:9)", 0x01),
            new EnumOption("Dot By Dot", 0x02),
            new EnumOption("4:3", 0x03),
            new EnumOption("14:9", 0x04),
            new EnumOption("Underscan", 0x05),
            new EnumOption("Pan Scan 16:9", 0x06)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x0C), PanelType.Video, "HDMI Input EQ",
        [
            new EnumOption("LOW", 0x02),
            new EnumOption("MIDDLE", 0x00),
            new EnumOption("HIGH", 0x01)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x02), PanelType.Video, "Output Format",
        [
            new EnumOption("1920x1080 60.00p", 0x0F),
            new EnumOption("1920x1080 59.94p", 0x10),
            new EnumOption("1920x1080 50.00p", 0x11),
            new EnumOption("1920x1080 30.00p", 0x12),
            new EnumOption("1920x1080 29.97p", 0x13),
            new EnumOption("1920x1080 25.00p", 0x14),
            new EnumOption("1920x1080 24.00p", 0x15),
            new EnumOption("1920x1080 23.98p", 0x16),
            new EnumOption("1920x1080 60.00i", 0x0C),
            new EnumOption("1920x1080 59.94i", 0x0D),
            new EnumOption("1920x1080 50.00i", 0x0E),
            new EnumOption("1280x720 60.00p", 0x06),
            new EnumOption("1280x720 59.94p", 0x07),
            new EnumOption("1280x720 50.00p", 0x08),
            new EnumOption("720x480 60.00p", 0x03),
            new EnumOption("720x480 59.94p", 0x04),
            new EnumOption("720x576 50.00p", 0x05),
            new EnumOption("720x480 59.94i", 0x01),
            new EnumOption("720x576 50.00i", 0x02)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x0A), PanelType.Video, "HDMI Output Color",
        [
            new EnumOption("YUV 444", 0x01),
            new EnumOption("YUV 422", 0x03),
            new EnumOption("RGB 444", 0x00)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x0F), PanelType.Video, "Video Output Range",
        [
            new EnumOption("Limit Range", 0x00),
            new EnumOption("Full Range", 0x01)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x13), PanelType.Video, "Edge Enhance",
        [
            new EnumOption("Disable", 0x00),
            new EnumOption("Min", 0x01),
            new EnumOption("Mid", 0x02),
            new EnumOption("Max", 0x03)
        ]),

        // Video panel, 61 type
        new EnumSettingDefinition(new SettingKey(MessageType.S61, 0x19), PanelType.Video, "Enable 3G Level-B",
        [
            new EnumOption("Disable", 0x00),
            new EnumOption("Enable", 0x01)
        ], template: [0x00, 0x01, 0x00, 0x00, 0x00, 0x00], valueOffset: 2),

        // Image Adjust panel
        new PositionSettingDefinition(new SettingKey(MessageType.S61, 0x11), PanelType.ImageAdjust, "Position"),
        new SizeSettingDefinition(new SettingKey(MessageType.S61, 0x12), PanelType.ImageAdjust, "Size"),

        // Audio panel
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x0B), PanelType.Audio, "Audio Mute",
        [
            new EnumOption("OFF", 0x00),
            new EnumOption("ON", 0x01)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x06), PanelType.Audio, "Audio Source",
        [
            new EnumOption("Internal", 0x01),
            new EnumOption("HDMI", 0x02),
            new EnumOption("SDI", 0x03),
            new EnumOption("L/R Audio", 0x00)
        ]),
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x10), PanelType.Audio, "Audio Level",
        [
            new EnumOption("SMTPE(USA)", 0x00),
            new EnumOption("EBU(Europe)", 0x01)
        ]),

        // Proc Amp panel
        new RangeSettingDefinition(new SettingKey(MessageType.C1, 0x02), PanelType.ProcAmp, "Brightness"),
        new RangeSettingDefinition(new SettingKey(MessageType.C1, 0x03), PanelType.ProcAmp, "Contrast"),
        new RangeSettingDefinition(new SettingKey(MessageType.C1, 0x04), PanelType.ProcAmp, "Hue"),
        new RangeSettingDefinition(new SettingKey(MessageType.C1, 0x05), PanelType.ProcAmp, "Saturation"),
        
        // Setting panel, c0 type
        new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x15), PanelType.Setting, "OSD Hide Mode",
        [
            new EnumOption("Disable", 0x00),
            new EnumOption("Enable", 0x01)
        ]),

        // Setting panel, 61 type
        new EnumSettingDefinition(new SettingKey(MessageType.S61, 0x17), PanelType.Setting, "DipSwitch 8(1) Mode",
        [
            new EnumOption("Keep Aspect Ratio", 0x00),
            new EnumOption("Pan Scan 16:9", 0x06)
        ], template: [0x00, 0x02, 0x00, 0x01, 0x00, 0x00], valueOffset: 2),
        new EnumSettingDefinition(new SettingKey(MessageType.S61, 0x22), PanelType.Setting, "No Signal Mode",
        [
            new EnumOption("Black Screen", 0x00),
            new EnumOption("Blue Screen", 0x01),
            new EnumOption("Disable Output", 0x02)
        ], template: [0x00, 0x01, 0x00, 0x00, 0x00, 0x00], valueOffset: 2)
    ];

    private static readonly Dictionary<SettingKey, Dac70SettingDefinition> _definitions =
        All.ToDictionary(definition => definition.Key);

    public static Dac70SettingDefinition Get(SettingKey key)
    {
        if (!_definitions.TryGetValue(key, out Dac70SettingDefinition? definition))
        {
            throw new KeyNotFoundException(
                $"No DAC-70 setting with message type 0x{(byte)key.Type:X2} and ID 0x{key.Id:X2}.");
        }

        return definition;
    }

    public static bool TryGet(SettingKey key, out Dac70SettingDefinition? definition)
    {
        return _definitions.TryGetValue(key, out definition);
    }
}
