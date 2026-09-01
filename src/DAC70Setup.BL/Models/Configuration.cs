using System.Collections.ObjectModel;
using DAC70Setup.Core;
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
        Settings.CollectionChanged += (_, _) => RaisePropertyChanged(nameof(SettingsCountText));
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

    public string DateCreatedText => Created.ToString("d");

    public string TimeCreatedText => Created.ToString("t");

    public string SettingsCountText => $"{Settings.Count} settings";

    public string CreatedText => $"{DateCreatedText} · {TimeCreatedText}";

    public ObservableCollection<Dac70SettingValue> Settings { get; } = new ObservableCollection<Dac70SettingValue>();
}
