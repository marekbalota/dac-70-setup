using System.Collections.ObjectModel;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models;

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
    }

    public Guid Id { get; }

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTime Created { get; }

    public string DateCreatedText => Created.ToString("d");

    public string TimeCreatedText => Created.ToString("t");

    public ObservableCollection<Dac70SettingValue> Settings { get; } = new ObservableCollection<Dac70SettingValue>();
}
