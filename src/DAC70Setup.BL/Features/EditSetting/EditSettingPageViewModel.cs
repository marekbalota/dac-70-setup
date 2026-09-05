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

    public EditSettingPageViewModel(IConfigurationsService configurationsService,
        INavigationService navigationService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _navigationService = navigationService;
        _localizationResourceManager = localizationResourceManager;

        SaveCommand = new DelegateCommand(async () =>
        {
            IsBusy = true;
            try
            {
                await _configurationsService.SaveSettingAsync(Configuration!, WorkingValue);

                await _navigationService.GoBackToRootAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }, () => !IsBusy)
            .ObservesProperty(() => IsBusy);

        CancelCommand = new DelegateCommand(async () =>
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
        }, () => !IsBusy)
            .ObservesProperty(() => IsBusy);
    }

    private Dac70SettingValue _workingValue = null!;
    public Dac70SettingValue WorkingValue
    {
        get => _workingValue;
        private set => SetProperty(ref _workingValue, value, () => RaisePropertyChanged(nameof(SettingName)));
    }

    public string SettingName => _workingValue?.Definition.Name ?? string.Empty;

    private Configuration? _configuration;
    public Configuration? Configuration
    {
        get => _configuration;
        private set => SetProperty(ref _configuration, value);
    }

    private string? _dependencyNoticeText;
    public string? DependencyNoticeText
    {
        get => _dependencyNoticeText;
        private set => SetProperty(ref _dependencyNoticeText, value);
    }

    private bool _isDependencyUnmet;
    public bool IsDependencyUnmet
    {
        get => _isDependencyUnmet;
        private set => SetProperty(ref _isDependencyUnmet, value);
    }

    public DelegateCommand SaveCommand { get; }

    public DelegateCommand CancelCommand { get; }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        Configuration = parameters.GetValue<Configuration>(EditSettingNavigationParameters.Configuration);

        if (parameters.TryGetValue(EditSettingNavigationParameters.SettingValue, out Dac70SettingValue? existingValue) && existingValue is not null)
        {
            WorkingValue = existingValue.Definition.CreateValue(existingValue.GetValueBytes());
        }
        else
        {
            var definition = parameters.GetValue<Dac70SettingDefinition>(EditSettingNavigationParameters.SettingDefinition);
            WorkingValue = definition.CreateValue();
        }

        EvaluateDependency();
    }

    private void EvaluateDependency()
    {
        SettingDependency? dependency = WorkingValue.Definition.Dependency;

        if (dependency is null)
        {
            IsDependencyUnmet = false;
            DependencyNoticeText = null;

            return;
        }

        string requiredSetting = dependency.RequiredSetting.Name;
        string requiredOption = dependency.RequiredOption.Name;

        Dac70SettingValue? required = Configuration!.Settings
            .FirstOrDefault(setting => setting.Definition.Key == dependency.RequiredSetting.Key);

        if (required is EnumSettingValue enumValue && enumValue.SelectedOption != dependency.RequiredOption)
        {
            IsDependencyUnmet = true;
            DependencyNoticeText = string.Format(
                _localizationResourceManager[nameof(EditSettingPageSR.EditSettingDependencyConflict)],
                requiredSetting, requiredOption, enumValue.SelectedOption.Name);
        }
        else
        {
            IsDependencyUnmet = false;
            DependencyNoticeText = string.Format(
                _localizationResourceManager[nameof(EditSettingPageSR.EditSettingDependencyInfo)],
                requiredSetting, requiredOption);
        }
    }
}
