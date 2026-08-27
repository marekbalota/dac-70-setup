using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Features.AddSetting;

public class AddSettingPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    private Configuration _configuration = null!;

    public AddSettingPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        SelectDefinitionCommand = new DelegateCommand<Dac70SettingDefinition>(
            async definition => await SelectDefinitionAsync(definition),
            _ => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    private IReadOnlyList<SettingDefinitionGroup> _availableSettings = [];
    public IReadOnlyList<SettingDefinitionGroup> AvailableSettings
    {
        get => _availableSettings;
        private set => SetProperty(ref _availableSettings, value);
    }

    public DelegateCommand<Dac70SettingDefinition> SelectDefinitionCommand { get; }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        _configuration = parameters.GetValue<Configuration>(EditSettingNavigationParameters.Configuration);
    }

    protected override async Task OnNavigatedToInt(INavigationParameters parameters)
    {
        await base.OnNavigatedToInt(parameters);

        var existingKeys = _configuration.Settings.Select(s => s.Definition.Key).ToHashSet();

        AvailableSettings = Dac70SettingCatalog.All
            .Where(d => !existingKeys.Contains(d.Key))
            .GroupBy(d => d.Panel)
            .Select(g => new SettingDefinitionGroup(g.Key, g))
            .ToList();
    }

    private async Task SelectDefinitionAsync(Dac70SettingDefinition definition)
    {
        IsBusy = true;

        try
        {
            var parameters = new NavigationParameters
            {
                { EditSettingNavigationParameters.Configuration, _configuration },
                { EditSettingNavigationParameters.SettingDefinition, definition }
            };

            await _navigationService.NavigateAsync(nameof(EditSettingPage), parameters);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
