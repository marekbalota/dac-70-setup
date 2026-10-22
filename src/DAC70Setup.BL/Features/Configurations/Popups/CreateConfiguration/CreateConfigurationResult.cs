using DAC70Setup.BL.Models;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public record CreateConfigurationResult(string Name, ConfigurationSource Source, Configuration? CopiedConfiguration,
    IReadOnlyList<Dac70SettingValue>? ImportedValues);
