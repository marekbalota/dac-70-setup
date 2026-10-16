using DAC70Setup.BL.Mappers;
using DAC70Setup.BL.Models;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services;

public class ConfigurationsService : IConfigurationsService
{
    private const string SelectedConfigurationIdKey = "SelectedConfigurationId";

    private readonly DataModel _dataModel;
    private readonly IConfigurationRepository _repository;
    private readonly ConfigurationMapper _configurationMapper;
    private readonly SettingMapper _settingMapper;

    public ConfigurationsService(DataModel dataModel,
        IConfigurationRepository repository,
        ConfigurationMapper configurationMapper,
        SettingMapper settingMapper)
    {
        _dataModel = dataModel;
        _repository = repository;
        _configurationMapper = configurationMapper;
        _settingMapper = settingMapper;
    }

    public async Task InitializeAsync()
    {
        var configurations = await _repository.GetAllAsync();

        _dataModel.Configurations.Clear();
        foreach (var configuration in configurations)
        {
            _dataModel.Configurations.Add(_configurationMapper.Map(configuration));
        }

        string storedId = Preferences.Default.Get(SelectedConfigurationIdKey, string.Empty);
        if (Guid.TryParse(storedId, out Guid id))
        {
            _dataModel.SelectedConfiguration = _dataModel.Configurations.FirstOrDefault(c => c.Id == id);
        }
    }

    public Task CreateNewConfiguration(string name)
    {
        return CreateNewConfiguration(name, []);
    }

    public async Task CreateNewConfiguration(string name, IEnumerable<Dac70SettingValue> values)
    {
        var configuration = new Configuration(name, DateTime.Now);

        foreach (var value in values)
        {
            configuration.Settings.Add(value.Definition.CreateValue(value.GetValueBytes()));
        }

        await _repository.AddConfigurationAsync(_configurationMapper.Map(configuration));

        _dataModel.Configurations.Add(configuration);
    }

    public async Task DeleteConfiguration(Configuration configuration)
    {
        await _repository.DeleteConfigurationAsync(configuration.Id);
        _dataModel.Configurations.Remove(configuration);

        if (_dataModel.SelectedConfiguration == configuration)
        {
            _dataModel.SelectedConfiguration = null;
            Preferences.Default.Remove(SelectedConfigurationIdKey);
        }
    }

    public void SelectConfiguration(Configuration configuration)
    {
        _dataModel.SelectedConfiguration = configuration;
        Preferences.Default.Set(SelectedConfigurationIdKey, configuration.Id.ToString());
    }

    public Task SaveSettingAsync(Configuration configuration, Dac70SettingValue value)
    {
        return SaveSettingsAsync(configuration, [value]);
    }

    public async Task SaveSettingsAsync(Configuration configuration, IEnumerable<Dac70SettingValue> values)
    {
        var valueList = values.ToList();

        await _repository.UpsertSettingsAsync(configuration.Id, valueList.Select(_settingMapper.Map).ToList());

        foreach (var value in valueList)
        {
            var valueBytes = value.GetValueBytes();
            Dac70SettingValue? existing = configuration.Settings.FirstOrDefault(s => s.Definition.Key == value.Definition.Key);

            if (existing != null)
            {
                existing.SetValueBytes(valueBytes);
            }
            else
            {
                configuration.Settings.Add(value.Definition.CreateValue(valueBytes));
            }
        }
    }

    public async Task DeleteSettingAsync(Configuration configuration, Dac70SettingValue value)
    {
        await _repository.DeleteSettingAsync(configuration.Id, value.Definition.Key);

        configuration.Settings.Remove(value);
    }
}
