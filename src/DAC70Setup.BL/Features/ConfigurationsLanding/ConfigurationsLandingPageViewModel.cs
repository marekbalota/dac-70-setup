using DAC70Setup.BL.Services.Commands;
using DAC70Setup.BL.ViewModels;

namespace DAC70Setup.BL.Features.ConfigurationsLanding;

public class ConfigurationsLandingPageViewModel : ViewModelBase
{
    public ConfigurationsLandingPageViewModel(IApplicationCommands applicationCommands)
    {
        ApplicationCommands = applicationCommands;
    }

    public IApplicationCommands ApplicationCommands { get; }
}
