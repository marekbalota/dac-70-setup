using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;
using SQLite;

namespace DAC70Setup.Data;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly Lazy<Task<SQLiteAsyncConnection>> _connection;

    public ConfigurationRepository(string databasePath)
    {
        _connection = new Lazy<Task<SQLiteAsyncConnection>>(() => InitializeAsync(databasePath));
    }

    private static async Task<SQLiteAsyncConnection> InitializeAsync(string databasePath)
    {
        var connection = new SQLiteAsyncConnection(databasePath);

        await connection.CreateTableAsync<ConfigurationRow>();
        await connection.CreateTableAsync<SettingRow>();

        return connection;
    }

    public async Task<IReadOnlyList<Configuration>> GetAllAsync()
    {
        var connection = await _connection.Value;

        var configRows = new List<ConfigurationRow>();
        var settingRows = new List<SettingRow>();

        await connection.RunInTransactionAsync(transaction =>
        {
            configRows.AddRange(transaction.Table<ConfigurationRow>());
            settingRows.AddRange(transaction.Table<SettingRow>());
        });

        Dictionary<Guid, List<SettingRow>> settingsByConfig = settingRows
            .GroupBy(s => s.ConfigId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<Configuration>();
        foreach (var row in configRows)
        {
            var configuration = new Configuration(row.Id, row.Name, row.Created);

            if (settingsByConfig.TryGetValue(row.Id, out List<SettingRow>? settings))
            {
                foreach (var setting in settings)
                {
                    var key = new SettingKey((MessageType)setting.MessageType, setting.SettingId);
                    Dac70SettingValue value = Dac70SettingCatalog.Get(key).CreateValue(setting.ValueBytes);
                    configuration.Settings.Add(value);
                }
            }

            result.Add(configuration);
        }

        return result;
    }

    public async Task AddConfigurationAsync(Configuration configuration)
    {
        var connection = await _connection.Value;

        await connection.InsertAsync(new ConfigurationRow
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Created = configuration.Created
        });
    }

    public async Task DeleteConfigurationAsync(Guid id)
    {
        var connection = await _connection.Value;

        await connection.RunInTransactionAsync(transaction =>
        {
            transaction.Table<SettingRow>().Delete(s => s.ConfigId == id);
            transaction.Delete<ConfigurationRow>(id);
        });
    }
}
