namespace DAC70Setup.Core.Models;

public sealed record ConfigurationDto(
    Guid Id,
    string Name,
    DateTime Created,
    IReadOnlyList<SettingDto> Settings);
