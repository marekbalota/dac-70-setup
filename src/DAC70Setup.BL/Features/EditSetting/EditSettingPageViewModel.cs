using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Features.EditSetting.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Navigation;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditSetting;

public partial class EditSettingPageViewModel : ObservableObject, IQueryAttributable
{
    private readonly ConfigurationsService _configurationsService;
    private readonly INavigationService _navigationService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public EditSettingPageViewModel(ConfigurationsService configurationsService,
        INavigationService navigationService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _navigationService = navigationService;
        _localizationResourceManager = localizationResourceManager;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SettingName))]
    public partial Dac70SettingValue? WorkingValue { get; private set; }

    public string SettingName => WorkingValue?.Definition.Name ?? string.Empty;

    [ObservableProperty]
    public partial Configuration? Configuration { get; private set; }

    [ObservableProperty]
    public partial string? DependencyNoticeText { get; private set; }

    [ObservableProperty]
    public partial bool IsDependencyUnmet { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Configuration = (Configuration)query[EditSettingNavigationParameters.Configuration];

        var settingValue = (Dac70SettingValue)query[EditSettingNavigationParameters.SettingValue];
        WorkingValue = settingValue.Definition.CreateValue(settingValue.GetValueBytes());

        EvaluateDependency();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _configurationsService.SaveSettingAsync(Configuration!, WorkingValue!);

        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _navigationService.GoBackAsync();
    }

    private void EvaluateDependency()
    {
        SettingDependency? dependency = WorkingValue!.Definition.Dependency;

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
