using System.Collections.ObjectModel;
using DAC70Setup.Core.Models.SettingTypes;

namespace DAC70Setup.Core.Models;

public class Configuration : NotifyPropertyChangedBase
{
    public Configuration(string name, DateTime created)
    {
        _name = name;
        Created = created;
    }

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTime Created { get; }

    public string DateCreatedText => Created.ToString("d");

    public string TimeCreatedText => Created.ToString("t");

    public ObservableCollection<IDac70Setting> Settings { get; } = new ObservableCollection<IDac70Setting>();
}
