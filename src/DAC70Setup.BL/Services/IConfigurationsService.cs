using DAC70Setup.BL.Models;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services;

public interface IConfigurationsService
{
    Task InitializeAsync();

    Task CreateNewConfiguration(string name);

    Task CreateNewConfiguration(string name, IEnumerable<Dac70SettingValue> values);

    Task DeleteConfiguration(Configuration configuration);

    void SelectConfiguration(Configuration configuration);

    Task SaveSettingAsync(Configuration configuration, Dac70SettingValue value);

    Task SaveSettingsAsync(Configuration configuration, IEnumerable<Dac70SettingValue> values);

    Task DeleteSettingAsync(Configuration configuration, Dac70SettingValue value);
}
