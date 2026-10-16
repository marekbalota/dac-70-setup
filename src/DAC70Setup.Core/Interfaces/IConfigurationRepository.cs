using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Interfaces;

public interface IConfigurationRepository
{
    Task<IReadOnlyList<ConfigurationDto>> GetAllAsync();

    Task AddConfigurationAsync(ConfigurationDto configuration);

    Task DeleteConfigurationAsync(Guid id);

    Task UpsertSettingsAsync(Guid configId, IReadOnlyList<SettingDto> settings);

    Task DeleteSettingAsync(Guid configId, SettingKey key);
}
