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

    public ConfigurationsService(DataModel dataModel,
        IConfigurationRepository repository,
        ConfigurationMapper configurationMapper)
    {
        _dataModel = dataModel;
        _repository = repository;
        _configurationMapper = configurationMapper;
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

    public async Task CreateNewConfiguration(string name)
    {
        var configuration = new Configuration(name, DateTime.Now);
        await _repository.AddConfigurationAsync(configuration.Id, configuration.Name, configuration.Created);
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
}
