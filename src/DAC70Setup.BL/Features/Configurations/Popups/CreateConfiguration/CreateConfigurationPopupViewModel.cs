using System.ComponentModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration.Resources;
using DAC70Setup.BL.Services.Device;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public partial class CreateConfigurationPopupViewModel : ObservableObject, IDisposable
{
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly IPopupService _popupService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public CreateConfigurationPopupViewModel(DeviceConnectionService deviceConnectionService,
        IPopupService popupService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _deviceConnectionService = deviceConnectionService;
        _popupService = popupService;
        _localizationResourceManager = localizationResourceManager;

        Name = string.Empty;
        DeviceDetail = new LocalizedString(() => string.Empty);

        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        UpdateDevice();
        IsFromDevice = IsDeviceAvailable;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsFromDevice { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChooseDeviceCommand))]
    public partial bool IsDeviceAvailable { get; private set; }

    [ObservableProperty]
    public partial LocalizedString DeviceDetail { get; private set; }

    [RelayCommand(CanExecute = nameof(IsDeviceAvailable))]
    private void ChooseDevice()
    {
        IsFromDevice = true;
    }

    [RelayCommand]
    private void ChooseEmpty()
    {
        IsFromDevice = false;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private Task CreateAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current,
            new CreateConfigurationResult(Name.Trim(), IsFromDevice));
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current);
    }

    private bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(Name);
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
            IsFromDevice = false;
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
