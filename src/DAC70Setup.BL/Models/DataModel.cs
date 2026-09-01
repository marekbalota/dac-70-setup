using System.Collections.ObjectModel;

namespace DAC70Setup.BL.Models;

public class DataModel : BindableBase
{
    public DataModel()
    {

    }

    public ObservableCollection<Configuration> Configurations { get; } = new ObservableCollection<Configuration>();

    private Configuration? _selectedConfiguration;
    public Configuration? SelectedConfiguration
    {
        get => _selectedConfiguration;
        set
        {
            Configuration? previous = _selectedConfiguration;

            if (SetProperty(ref _selectedConfiguration, value))
            {
                previous?.IsActive = false;
                value?.IsActive = true;
            }
        }
    }
}
