using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.BL.Mappers;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services;

public partial class ConfigurationsService : ObservableObject
{
    private const string SelectedConfigurationIdKey = "SelectedConfigurationId";

    private readonly IConfigurationRepository _repository;
    private readonly ConfigurationMapper _configurationMapper;
    private readonly SettingMapper _settingMapper;

    public ConfigurationsService(IConfigurationRepository repository,
        ConfigurationMapper configurationMapper,
        SettingMapper settingMapper)
    {
        _repository = repository;
        _configurationMapper = configurationMapper;
        _settingMapper = settingMapper;
    }

    public ObservableCollection<Configuration> Configurations { get; } = [];

    [ObservableProperty]
    public partial Configuration? SelectedConfiguration { get; private set; }

    partial void OnSelectedConfigurationChanged(Configuration? oldValue, Configuration? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsActive = false;
        }

        if (newValue != null)
        {
            newValue.IsActive = true;
        }
    }

    public async Task InitializeAsync()
    {
        var configurations = await _repository.GetAllAsync();

        Configurations.Clear();
        foreach (var configuration in configurations)
        {
            Configurations.Add(_configurationMapper.Map(configuration));
        }

        string storedId = Preferences.Default.Get(SelectedConfigurationIdKey, string.Empty);
        if (Guid.TryParse(storedId, out Guid id))
        {
            SelectedConfiguration = Configurations.FirstOrDefault(configuration => configuration.Id == id);
        }
    }

    public Task CreateNewConfigurationAsync(string name)
    {
        return CreateNewConfigurationAsync(name, []);
    }

    private async Task CreateNewConfigurationAsync(string name, IEnumerable<Dac70SettingValue> values)
    {
        var configuration = new Configuration(name, DateTime.Now);

        foreach (var value in values)
        {
            configuration.Settings.Add(value.Definition.CreateValue(value.GetValueBytes()));
        }

        await _repository.AddConfigurationAsync(_configurationMapper.Map(configuration));

        Configurations.Add(configuration);
    }

    public Task CreateFromDeviceAsync(string name, DeviceSnapshot snapshot)
    {
        var values = Dac70SettingCatalog.All
            .Select(definition => snapshot.Values.GetValueOrDefault(definition.Key))
            .OfType<Dac70SettingValue>()
            .ToList();

        return CreateNewConfigurationAsync(name, values);
    }

    public async Task DeleteConfigurationAsync(Configuration configuration)
    {
        await _repository.DeleteConfigurationAsync(configuration.Id);
        Configurations.Remove(configuration);

        if (SelectedConfiguration == configuration)
        {
            SelectedConfiguration = null;
            Preferences.Default.Remove(SelectedConfigurationIdKey);
        }
    }

    public void SelectConfiguration(Configuration configuration)
    {
        SelectedConfiguration = configuration;
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
