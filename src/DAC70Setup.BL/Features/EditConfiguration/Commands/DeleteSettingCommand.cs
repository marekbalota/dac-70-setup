using System.Collections.Specialized;
using DAC70Setup.BL.Features.EditConfiguration.Resources;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Services;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditConfiguration.Commands;

public class DeleteSettingCommand : ActiveCommandBase<EditConfigurationPageViewModel>
{
    private readonly IConfigurationsService _configurationsService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public DeleteSettingCommand(IConfigurationsService configurationsService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _configurationsService = configurationsService;
        _pageDialogService = pageDialogService;
        _localizationResourceManager = localizationResourceManager;
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();

        ObserveProperty(() => ViewModel.IsBusy);

        ViewModel.SelectedSettings.CollectionChanged += OnSelectedSettingsChanged;
    }

    private void OnSelectedSettingsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaiseCanExecuteChanged();
    }

    protected override bool CanExecute(object? parameter)
    {
        return !ViewModel.IsBusy && ViewModel.SelectedSettings.Count > 0;
    }

    protected override async void Execute(object? parameter)
    {
        await RunBusyAsync(async () =>
        {
            var configuration = ViewModel.Model.SelectedConfiguration;
            var settings = ViewModel.SelectedSettings.Cast<Dac70SettingValue>().ToList();

            if (configuration == null || settings.Count == 0)
            {
                return;
            }

            bool confirmed = await _pageDialogService.DisplayAlertAsync(
                _localizationResourceManager[nameof(EditConfigurationPageSR.DeleteSettingsConfirmTitle)],
                _localizationResourceManager[nameof(EditConfigurationPageSR.DeleteSettingsConfirmMessage), settings.Count],
                _localizationResourceManager[nameof(EditConfigurationPageSR.DeleteSettingsConfirmAccept)],
                _localizationResourceManager[nameof(EditConfigurationPageSR.DeleteSettingsConfirmCancel)]);

            if (!confirmed)
            {
                return;
            }

            await _configurationsService.DeleteSettingsAsync(configuration, settings);

            ViewModel.SelectedSettings.Clear();
        });
    }

    protected override void OnViewModelUnset()
    {
        base.OnViewModelUnset();

        ViewModel.SelectedSettings.CollectionChanged -= OnSelectedSettingsChanged;
    }
}
