using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.EditConfiguration.Commands;
using DAC70Setup.BL.Features.EditConfiguration.Resources;
using DAC70Setup.BL.Features.EditSetting;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Commands;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.EditConfiguration;

public class EditConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IApplicationCommands _applicationCommands;
    private readonly IConfigurationsService _configurationsService;
    private readonly IPageDialogService _pageDialogService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    private Configuration? _observedConfiguration;

    public EditConfigurationPageViewModel(DataModel model,
        INavigationService navigationService,
        IApplicationCommands applicationCommands,
        IConfigurationsService configurationsService,
        IPageDialogService pageDialogService,
        ILocalizationResourceManager localizationResourceManager,
        AddSettingCommand addSettingCommand,
        BeginSelectionCommand beginSelectionCommand)
    {
        Model = model;
        _navigationService = navigationService;
        _applicationCommands = applicationCommands;
        _configurationsService = configurationsService;
        _pageDialogService = pageDialogService;
        _localizationResourceManager = localizationResourceManager;

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

        DeleteSelectionCommand = new DelegateCommand(async () =>
        {
            IsBusy = true;
            try
            {
                var configuration = Model.SelectedConfiguration;
                var settings = SelectedSettings.ToList();

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

                IsSelecting = false;
            }
            finally
            {
                IsBusy = false;
            }
        }, () => !IsBusy && IsSelecting && SelectedSettings.Count > 0)
            .ObservesProperty(() => IsBusy)
            .ObservesProperty(() => IsSelecting);

        // TODO: read the device's current configuration into a new configuration.
        DownloadFromDeviceCommand = new DelegateCommand(() => { });

        AddSettingCommand = addSettingCommand;
        BeginSelectionCommand = beginSelectionCommand;

        _applicationCommands.AddCommand.RegisterCommand(AddSettingCommand);
        _applicationCommands.RemoveCommand.RegisterCommand(BeginSelectionCommand);

        SelectedSettings.CollectionChanged += OnSelectedSettingsChanged;
        Model.PropertyChanged += OnModelPropertyChanged;

        ObserveSelectedConfiguration();
    }

    public DataModel Model { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public DelegateCommand DownloadFromDeviceCommand { get; }

    public DelegateCommand<Dac70SettingValue> OpenSettingCommand { get; }

    public DelegateCommand DeleteSelectionCommand { get; }

    public CommandBase AddSettingCommand { get; }

    public CommandBase BeginSelectionCommand { get; }

    public ObservableCollection<Dac70SettingValue> SelectedSettings { get; } = [];

    private bool _isSelecting;
    public bool IsSelecting
    {
        get => _isSelecting;
        set => SetProperty(ref _isSelecting, value);
    }

    /// <summary>
    /// Guards the toolbar's remove button: there is nothing to pick from an
    /// empty configuration. The list draws its own empty message.
    /// </summary>
    public bool HasNoSettings => Model.SelectedConfiguration == null
        || Model.SelectedConfiguration.Settings.Count == 0;

    private void OnSelectedSettingsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteSelectionCommand.RaiseCanExecuteChanged();
    }

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

        IsSelecting = false;

        RaisePropertyChanged(nameof(HasNoSettings));
    }

    private void OnSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(HasNoSettings));
    }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        AddSettingCommand.Initialize(this);
        BeginSelectionCommand.Initialize(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Model.PropertyChanged -= OnModelPropertyChanged;
        SelectedSettings.CollectionChanged -= OnSelectedSettingsChanged;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
            _observedConfiguration = null;
        }

        _applicationCommands.AddCommand.UnregisterCommand(AddSettingCommand);
        _applicationCommands.RemoveCommand.UnregisterCommand(BeginSelectionCommand);

        AddSettingCommand.Destroy();
        BeginSelectionCommand.Destroy();
    }
}
