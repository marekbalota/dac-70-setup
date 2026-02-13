using DAC70Setup.BL.Models;
using DAC70Setup.BL.ViewModels;

namespace DAC70Setup.BL.Features.Configurations;

public class ConfigurationsPageViewModel : TabViewModelBase
{
    public ConfigurationsPageViewModel(DataModel model)
    {
        Model = model;
    }

    public DataModel Model { get; }

    private Configuration _selectedConfiguration;
    public Configuration SelectedConfiguration
    {
        get => _selectedConfiguration;
        set => SetProperty(ref _selectedConfiguration, value);
    }
}
