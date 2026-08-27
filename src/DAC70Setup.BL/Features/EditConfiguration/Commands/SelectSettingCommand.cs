using System.Collections.Specialized;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Features.EditConfiguration.Commands;

public class SelectSettingCommand : ActiveCommandBase<EditConfigurationPageViewModel>
{
    private readonly INavigationService _navigationService;

    public SelectSettingCommand(INavigationService navigationService)
    {
        _navigationService = navigationService;
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
        return !ViewModel.IsBusy && ViewModel.SelectedSettings.Count == 1;
    }

    protected override async void Execute(object? parameter)
    {
        await RunBusyAsync(async () =>
        {
            var configuration = ViewModel.Model.SelectedConfiguration;
            var setting = ViewModel.SelectedSettings.Cast<Dac70SettingValue>().FirstOrDefault();

            if (configuration == null || setting == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { EditSettingNavigationParameters.Configuration, configuration },
                { EditSettingNavigationParameters.SettingValue, setting }
            };

            await _navigationService.NavigateAsync(nameof(EditSettingPage), parameters);
        });
    }

    protected override void OnViewModelUnset()
    {
        base.OnViewModelUnset();

        ViewModel.SelectedSettings.CollectionChanged -= OnSelectedSettingsChanged;
    }
}
