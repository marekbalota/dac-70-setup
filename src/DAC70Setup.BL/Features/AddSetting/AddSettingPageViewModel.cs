using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Features.AddSetting;

public class AddSettingPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public AddSettingPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        SelectDefinitionCommand = new DelegateCommand<Dac70SettingDefinition>(async definition =>
        {
            IsBusy = true;
            try
            {
                if (Configuration == null)
                {
                    return;
                }

                var parameters = new NavigationParameters
                {
                    { EditSettingNavigationParameters.Configuration, Configuration },
                    { EditSettingNavigationParameters.SettingDefinition, definition }
                };

                await _navigationService.NavigateAsync(nameof(EditSettingPage), parameters);
            }
            finally
            {
                IsBusy = false;
            }
        }, _ => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    private Configuration? _configuration;
    public Configuration? Configuration
    {
        get => _configuration;
        private set => SetProperty(ref _configuration, value);
    }

    private IReadOnlyList<SettingDefinitionGroup> _availableSettings = [];
    public IReadOnlyList<SettingDefinitionGroup> AvailableSettings
    {
        get => _availableSettings;
        private set => SetProperty(ref _availableSettings, value, () => RaisePropertyChanged(nameof(IsEmpty)));
    }

    public bool IsEmpty => _availableSettings.Count == 0;

    public DelegateCommand<Dac70SettingDefinition> SelectDefinitionCommand { get; }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        Configuration = parameters.GetValue<Configuration>(EditSettingNavigationParameters.Configuration);

        AvailableSettings = Configuration.GetAvailableDefinitions()
            .GroupBy(d => d.Panel)
            .Select(g => new SettingDefinitionGroup(g.Key, g))
            .ToList();
    }
}
