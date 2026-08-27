using DAC70Setup.BL.Features.EditSetting.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditSetting;

public class EditSettingPageViewModel : ViewModelBase
{
    private readonly IConfigurationsService _configurationsService;
    private readonly INavigationService _navigationService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    private Configuration _configuration = null!;
    private Dac70SettingValue? _existingValue;

    public EditSettingPageViewModel(IConfigurationsService configurationsService,
        INavigationService navigationService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _navigationService = navigationService;
        _localizationResourceManager = localizationResourceManager;

        SaveCommand = new DelegateCommand(async () => await SaveAsync(), () => !IsBusy)
            .ObservesProperty(() => IsBusy);
        CancelCommand = new DelegateCommand(async () => await CancelAsync(), () => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    private Dac70SettingValue _workingValue = null!;
    public Dac70SettingValue WorkingValue
    {
        get => _workingValue;
        private set => SetProperty(ref _workingValue, value, () => RaisePropertyChanged(nameof(SettingName)));
    }

    public string SettingName => _workingValue?.Definition.Name ?? string.Empty;

    private string _pageTitle = string.Empty;
    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public DelegateCommand SaveCommand { get; }

    public DelegateCommand CancelCommand { get; }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        _configuration = parameters.GetValue<Configuration>(EditSettingNavigationParameters.Configuration);

        if (parameters.TryGetValue(EditSettingNavigationParameters.SettingValue, out _existingValue) && _existingValue is not null)
        {
            WorkingValue = _existingValue.Definition.CreateValue(_existingValue.GetValueBytes());
        }
        else
        {
            var definition = parameters.GetValue<Dac70SettingDefinition>(EditSettingNavigationParameters.SettingDefinition);
            WorkingValue = definition.CreateValue();
        }

        PageTitle = _existingValue != null
            ? _localizationResourceManager[nameof(EditSettingPageSR.EditSettingEditTitle)]
            : _localizationResourceManager[nameof(EditSettingPageSR.EditSettingAddTitle)];
    }

    private async Task SaveAsync()
    {
        IsBusy = true;

        try
        {
            await _configurationsService.SaveSettingAsync(_configuration, WorkingValue);

            await _navigationService.GoBackToRootAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        IsBusy = true;

        try
        {
            await _navigationService.GoBackAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
