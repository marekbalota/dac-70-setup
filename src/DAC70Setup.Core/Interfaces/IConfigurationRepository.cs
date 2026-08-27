using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Interfaces;

public interface IConfigurationRepository
{
    Task<IReadOnlyList<ConfigurationDto>> GetAllAsync();

    Task AddConfigurationAsync(Guid id, string name, DateTime created);

    Task DeleteConfigurationAsync(Guid id);

    Task UpsertSettingAsync(Guid configId, SettingKey key, byte[] valueBytes);

    Task DeleteSettingsAsync(Guid configId, IEnumerable<SettingKey> keys);
}
