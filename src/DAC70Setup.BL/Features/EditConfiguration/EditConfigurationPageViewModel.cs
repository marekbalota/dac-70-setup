using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DAC70Setup.BL.Controls.DeviceStatus;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.EditConfiguration.Controls;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditConfiguration;

public class EditConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;

    private Configuration? _observedConfiguration;

    public EditConfigurationPageViewModel(DataModel model, INavigationService navigationService,
        DeviceConnectionService deviceConnectionService, ILocalizationResourceManager localizationResourceManager)
    {
        Model = model;
        _navigationService = navigationService;

        DeviceStatus = new DeviceStatusBarViewModel(deviceConnectionService, localizationResourceManager);

        GoToConfigurationsCommand = new DelegateCommand(async () =>
            await _navigationService.SelectTabAsync(nameof(ConfigurationsPage)));

        OpenSettingCommand = new DelegateCommand<Dac70SettingValue>(async setting =>
        {
            IsBusy = true;
            try
            {
                var configuration = Model.SelectedConfiguration;
                if (configuration == null)
                {
                    return;
                }

                var parameters = new NavigationParameters
                {
                    { EditSettingNavigationParameters.Configuration, configuration },
                    { EditSettingNavigationParameters.SettingValue, setting }
                };

                await _navigationService.NavigateAsync(nameof(EditSettingPage), parameters);
            }
            finally
            {
                IsBusy = false;
            }
        }, _ => !IsBusy)
            .ObservesProperty(() => IsBusy);

        Model.PropertyChanged += OnModelPropertyChanged;

        ObserveSelectedConfiguration();
    }

    public DataModel Model { get; }

    public DeviceStatusBarViewModel DeviceStatus { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public DelegateCommand<Dac70SettingValue> OpenSettingCommand { get; }

    public ObservableCollection<SettingValueGroup> Settings { get; } = [];

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataModel.SelectedConfiguration))
        {
            ObserveSelectedConfiguration();
        }
    }

    private void ObserveSelectedConfiguration()
    {
        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
        }

        _observedConfiguration = Model.SelectedConfiguration;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged += OnSettingsCollectionChanged;
        }

        RebuildSettings();
    }

    private void OnSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildSettings();
    }

    private void RebuildSettings()
    {
        Settings.Clear();

        if (_observedConfiguration == null)
        {
            return;
        }

        var groups = _observedConfiguration.GetDisplaySettings()
            .GroupBy(value => value.Definition.Panel)
            .OrderBy(group => group.Key);

        foreach (var group in groups)
        {
            Settings.Add(new SettingValueGroup(group.Key, group));
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Model.PropertyChanged -= OnModelPropertyChanged;
        DeviceStatus.Dispose();

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
            _observedConfiguration = null;
        }
    }
}
