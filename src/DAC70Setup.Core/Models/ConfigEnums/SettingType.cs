namespace DAC70Setup.Core.Models.ConfigEnums;

public enum SettingType : byte
{
    // C0 Type
    InputSource = 0x04,
    AspectRatio = 0x07,
    HdmiInputEq = 0x0C,
    OutputFormat = 0x02,
    HdmiOutputColor = 0x0A,
    VideoOutputRange = 0x0F,
    EdgeEnhance = 0x13,
    AudioMute = 0x0B,
    AudioSource = 0x06,
    AudioLevel = 0x10,
    OsdHideMode = 0x15,

    // C1 Type
    Brightness = 0x02,
    Contrast = 0x03,
    Hue = 0x04,
    Saturation = 0x05,

    // 61 Type
    Enable3GLevelB = 0x19,
    Position = 0x11,
    Size = 0x12,
    DipSwitch8Mode = 0x17,
    NoSignalMode = 0x22
}
