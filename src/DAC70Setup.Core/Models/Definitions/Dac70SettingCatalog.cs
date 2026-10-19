using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Models.Definitions;

public static class Dac70SettingCatalog
{
    public static IReadOnlyList<Dac70SettingDefinition> All { get; } = Create();

    private static IReadOnlyList<Dac70SettingDefinition> Create()
    {
        var vga = new EnumOption("VGA", 0x02);
        var inputSource = new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x04), PanelType.Video, "Input Source",
        [
            new EnumOption("HDMI", 0x01),
            vga,
            new EnumOption("SDI", 0x04)
        ]);

        var underscan = new EnumOption("Underscan", 0x05);
        var aspectRatio = new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x07), PanelType.Video, "Aspect Ratio",
        [
            new EnumOption("Keep Aspect Ratio", 0x00),
            new EnumOption("Full Screen (16:9)", 0x01),
            new EnumOption("Dot By Dot", 0x02),
            new EnumOption("4:3", 0x03),
            new EnumOption("14:9", 0x04),
            underscan,
            new EnumOption("Pan Scan 16:9", 0x06)
        ]);

        return
        [
            // Video panel, c0 type
            inputSource,
            aspectRatio,
            new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x0C), PanelType.Video, "HDMI Input EQ",
            [
                new EnumOption("LOW", 0x02),
                new EnumOption("MIDDLE", 0x00),
                new EnumOption("HIGH", 0x01)
            ]),
            new EnumSettingDefinition(new SettingKey(MessageType.C0, 0x02), PanelType.Video, "Output Format",
            [
                new EnumOption("1920x1080p 60.00", 0x0F),
                new EnumOption("1920x1080p 59.94", 0x10),
                new EnumOption("1920x1080p 50.00", 0x11),
                new EnumOption("1920x1080p 30.00", 0x12),
                new EnumOption("1920x1080p 29.97", 0x13),
                new EnumOption("1920x1080p 25.00", 0x14),
                new EnumOption("1920x1080p 24.00", 0x15),
                new EnumOption("1920x1080p 23.98", 0x16),
                new EnumOption("1920x1080i 60.00", 0x0C),
                new EnumOption("1920x1080i 59.94", 0x0D),
                new EnumOption("1920x1080i 50.00", 0x0E),
                new EnumOption("1280x720p 60.00", 0x06),
                new EnumOption("1280x720p 59.94", 0x07),
                new EnumOption("1280x720p 50.00", 0x08),
                new EnumOption("720x480p 60.00", 0x03),
                new EnumOption("720x480p 59.94", 0x04),
                new EnumOption("720x576p 50.00", 0x05),
                new EnumOption("720x480i 59.94", 0x01),
                new EnumOption("720x576i 50.00", 0x02)
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
            new PositionSettingDefinition(new SettingKey(MessageType.S61, 0x11), PanelType.ImageAdjust, "Position",
                dependency: new SettingDependency(inputSource, vga)),
            new SizeSettingDefinition(new SettingKey(MessageType.S61, 0x12), PanelType.ImageAdjust, "Size",
                dependency: new SettingDependency(aspectRatio, underscan)),

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
    }

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
