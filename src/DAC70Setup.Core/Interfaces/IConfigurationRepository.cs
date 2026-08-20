using DAC70Setup.Core.Models;

namespace DAC70Setup.Core.Interfaces;

public interface IConfigurationRepository
{
    Task<IReadOnlyList<Configuration>> GetAllAsync();

    Task AddConfigurationAsync(Configuration configuration);

    Task DeleteConfigurationAsync(Guid id);
}
