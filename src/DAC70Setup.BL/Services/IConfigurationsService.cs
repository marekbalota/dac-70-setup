using DAC70Setup.Core.Models;

namespace DAC70Setup.BL.Services;

public interface IConfigurationsService
{
    Task InitializeAsync();

    Task CreateNewConfiguration(string name);

    Task DeleteConfiguration(Configuration configuration);

    void SelectConfiguration(Configuration configuration);
}
