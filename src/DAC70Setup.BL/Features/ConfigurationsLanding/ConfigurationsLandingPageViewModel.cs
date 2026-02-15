using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Commands;

namespace DAC70Setup.BL.Features.ConfigurationsLanding;

public class ConfigurationsLandingPageViewModel : ViewModelBase
{
    public ConfigurationsLandingPageViewModel(IApplicationCommands applicationCommands)
    {
        ApplicationCommands = applicationCommands;
    }

    public IApplicationCommands ApplicationCommands { get; }
}
