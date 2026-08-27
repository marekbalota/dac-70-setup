using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Protocol;
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

    public async Task<IReadOnlyList<ConfigurationDto>> GetAllAsync()
    {
        var connection = await _connection.Value;

        var configRows = new List<ConfigurationRow>();
        var settingRows = new List<SettingRow>();

        await connection.RunInTransactionAsync(transaction =>
        {
            configRows.AddRange(transaction.Table<ConfigurationRow>());
            settingRows.AddRange(transaction.Table<SettingRow>());
        });

        Dictionary<Guid, List<SettingDto>> settingsByConfig = configRows
            .ToDictionary(row => row.Id, _ => new List<SettingDto>());

        foreach (var row in settingRows)
        {
            if (settingsByConfig.TryGetValue(row.ConfigId, out List<SettingDto>? settings))
            {
                var key = new SettingKey((MessageType)row.MessageType, row.SettingId);
                settings.Add(new SettingDto(key, row.ValueBytes));
            }
        }

        return configRows
            .Select(row => new ConfigurationDto(row.Id, row.Name, row.Created, settingsByConfig[row.Id]))
            .ToList();
    }

    public async Task AddConfigurationAsync(Guid id, string name, DateTime created)
    {
        var connection = await _connection.Value;

        await connection.InsertAsync(new ConfigurationRow
        {
            Id = id,
            Name = name,
            Created = created
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

    public async Task UpsertSettingAsync(Guid configId, SettingKey key, byte[] valueBytes)
    {
        var connection = await _connection.Value;
        byte messageType = (byte)key.Type;
        byte settingId = key.Id;

        await connection.RunInTransactionAsync(transaction =>
        {
            SettingRow? existing = transaction.Table<SettingRow>()
                .FirstOrDefault(s => s.ConfigId == configId && s.MessageType == messageType && s.SettingId == settingId);

            if (existing != null)
            {
                existing.ValueBytes = valueBytes;
                transaction.Update(existing);
            }
            else
            {
                transaction.Insert(new SettingRow
                {
                    ConfigId = configId,
                    MessageType = messageType,
                    SettingId = settingId,
                    ValueBytes = valueBytes
                });
            }
        });
    }

    public async Task DeleteSettingsAsync(Guid configId, IEnumerable<SettingKey> keys)
    {
        var connection = await _connection.Value;
        var keySet = keys.ToHashSet();

        await connection.RunInTransactionAsync(transaction =>
        {
            foreach (var key in keySet)
            {
                transaction.Table<SettingRow>().Delete(s =>
                    s.ConfigId == configId && s.MessageType == (byte)key.Type && s.SettingId == key.Id);
            }
        });
    }
}
