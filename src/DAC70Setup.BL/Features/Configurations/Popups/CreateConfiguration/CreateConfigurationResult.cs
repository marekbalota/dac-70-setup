using DAC70Setup.BL.Models;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public record CreateConfigurationResult(string Name, ConfigurationSource Source, Configuration? CopiedConfiguration);
