namespace DAC70Setup.Core.Models.Definitions;

/// <summary>
/// A hardware precondition: the device only honours the setting while
/// <see cref="RequiredSetting"/> holds <see cref="RequiredOption"/>. Sending it
/// otherwise is not an error — the device simply ignores it.
/// </summary>
public sealed record SettingDependency(EnumSettingDefinition RequiredSetting, EnumOption RequiredOption);
