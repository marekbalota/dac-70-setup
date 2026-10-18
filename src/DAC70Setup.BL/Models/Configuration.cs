using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Models;

public partial class Configuration : ObservableObject
{
    public Configuration(string name, DateTime created) : this(Guid.NewGuid(), name, created)
    {
    }

    public Configuration(Guid id, string name, DateTime created)
    {
        Id = id;
        Name = name;
        Created = created;
        Settings.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SettingsCountText));
        };
    }

    public Guid Id { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    public DateTime Created { get; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    // TODO: this needs to support translation
    public string SettingsCountText => $"{Settings.Count} settings";

    public ObservableCollection<Dac70SettingValue> Settings { get; } = new ObservableCollection<Dac70SettingValue>();
}
