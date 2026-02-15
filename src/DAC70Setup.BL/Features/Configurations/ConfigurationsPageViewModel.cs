using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Commands;

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
