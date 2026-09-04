using System.Collections.ObjectModel;
using DAC70Setup.Core;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Models;

public class Configuration : NotifyPropertyChangedBase
{
    public Configuration(string name, DateTime created)
        : this(Guid.NewGuid(), name, created)
    {
    }

    public Configuration(Guid id, string name, DateTime created)
    {
        Id = id;
        _name = name;
        Created = created;
        Settings.CollectionChanged += (_, _) =>
        {
            RaisePropertyChanged(nameof(SettingsCountText));
            RaisePropertyChanged(nameof(HasAvailableSettings));
        };
    }

    public Guid Id { get; }

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTime Created { get; }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // TODO: this needs to support translation
    public string SettingsCountText => $"{Settings.Count} settings";

    public ObservableCollection<Dac70SettingValue> Settings { get; } = new ObservableCollection<Dac70SettingValue>();

    public bool HasAvailableSettings => GetAvailableDefinitions().Any();

    public IEnumerable<Dac70SettingDefinition> GetAvailableDefinitions()
    {
        var existingKeys = Settings.Select(s => s.Definition.Key).ToHashSet();

        return Dac70SettingCatalog.All.Where(d => !existingKeys.Contains(d.Key));
    }
}
