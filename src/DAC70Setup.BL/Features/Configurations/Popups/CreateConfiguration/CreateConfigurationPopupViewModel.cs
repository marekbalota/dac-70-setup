using System.ComponentModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public partial class CreateConfigurationPopupViewModel : ObservableObject, IQueryAttributable, IDisposable
{
    public const string CopiedConfigurationParameter = "CopiedConfiguration";

    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly IPopupService _popupService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public CreateConfigurationPopupViewModel(ConfigurationsService configurationsService,
        DeviceConnectionService deviceConnectionService,
        IPopupService popupService,
        ILocalizationResourceManager localizationResourceManager)
    {
        ConfigurationsService = configurationsService;
        _deviceConnectionService = deviceConnectionService;
        _popupService = popupService;
        _localizationResourceManager = localizationResourceManager;

        Name = string.Empty;
        DeviceDetail = new LocalizedString(() => string.Empty);
        CopiedConfiguration = configurationsService.SelectedConfiguration ?? configurationsService.Configurations.FirstOrDefault();

        string copyDetailKey = configurationsService.Configurations.Count > 0
            ? nameof(CreateConfigurationPopupSR.NewConfigurationCopyDetail)
            : nameof(CreateConfigurationPopupSR.NewConfigurationCopyUnavailable);
        CopyDetail = new LocalizedString(() => _localizationResourceManager[copyDetailKey]);

        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        UpdateDevice();

        if (IsDeviceAvailable)
        {
            Source = ConfigurationSource.Device;
        }
    }

    public ConfigurationsService ConfigurationsService { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeviceChosen), nameof(IsCopyChosen), nameof(IsEmptyChosen))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial ConfigurationSource Source { get; set; }

    public bool IsDeviceChosen => Source == ConfigurationSource.Device;

    public bool IsCopyChosen => Source == ConfigurationSource.Copy;

    public bool IsEmptyChosen => Source == ConfigurationSource.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial Configuration? CopiedConfiguration { get; set; }

    public LocalizedString CopyDetail { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChooseDeviceCommand))]
    public partial bool IsDeviceAvailable { get; private set; }

    [ObservableProperty]
    public partial LocalizedString DeviceDetail { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(CopiedConfigurationParameter, out object? value) && value is Configuration configuration)
        {
            CopiedConfiguration = configuration;
        }
    }

    [RelayCommand(CanExecute = nameof(IsDeviceAvailable))]
    private void ChooseDevice()
    {
        Source = ConfigurationSource.Device;
    }

    [RelayCommand(CanExecute = nameof(CanChooseCopy))]
    private void ChooseCopy()
    {
        Source = ConfigurationSource.Copy;
    }

    [RelayCommand]
    private void ChooseEmpty()
    {
        Source = ConfigurationSource.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private Task CreateAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current,
            new CreateConfigurationResult(Name.Trim(), Source, IsCopyChosen ? CopiedConfiguration : null));
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current);
    }

    private bool CanChooseCopy()
    {
        return ConfigurationsService.Configurations.Count > 0;
    }

    private bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(Name) && (!IsCopyChosen || CopiedConfiguration != null);
    }

    private void OnDeviceConnectionServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateDevice();
    }

    private void UpdateDevice()
    {
        IsDeviceAvailable = _deviceConnectionService.SelectedDevice != null && _deviceConnectionService.CanSend;

        string detailKey;
        if (!IsDeviceAvailable)
        {
            if (IsDeviceChosen)
            {
                Source = ConfigurationSource.Empty;
            }

            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceUnavailable);
        }
        else if (_deviceConnectionService.SelectedDevice?.Snapshot.LastFullSync != null)
        {
            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceSynced);
        }
        else
        {
            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceNotSynced);
        }

        DeviceDetail = new LocalizedString(() => _localizationResourceManager[detailKey]);
    }

    public void Dispose()
    {
        _deviceConnectionService.PropertyChanged -= OnDeviceConnectionServiceChanged;
    }
}
