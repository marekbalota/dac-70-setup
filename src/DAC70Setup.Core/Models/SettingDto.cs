using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Models;

public sealed record SettingDto(SettingKey Key, byte[] ValueBytes);
