using DAC70Setup.BL.Models;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services;

public interface IConfigurationsService
{
    Task InitializeAsync();

    Task CreateNewConfiguration(string name);

    Task DeleteConfiguration(Configuration configuration);

    void SelectConfiguration(Configuration configuration);

    Task SaveSettingAsync(Configuration configuration, Dac70SettingValue value);

    Task DeleteSettingsAsync(Configuration configuration, IReadOnlyList<Dac70SettingValue> values);
}
