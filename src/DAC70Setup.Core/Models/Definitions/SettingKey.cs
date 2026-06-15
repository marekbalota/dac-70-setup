using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Models.Definitions;

public readonly record struct SettingKey(MessageType Type, byte Id);
