using System.Collections.ObjectModel;
using System.ComponentModel;
using AsyncAwaitBestPractices;
using DAC70Setup.BL.Controls.DeviceStatus.Resources;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Interfaces;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.DeviceStatus;

public class DeviceStatusBarViewModel : BindableBase, IDisposable
{
    private const string DeviceName = "DAC-70";

    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    private Func<Task>? _action;

    private DeviceSnapshot? _observedSnapshot;

    public DeviceStatusBarViewModel(DeviceConnectionService deviceConnectionService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _deviceConnectionService = deviceConnectionService;
        _localizationResourceManager = localizationResourceManager;

        ActionCommand = new AsyncDelegateCommand(() => _action?.Invoke() ?? Task.CompletedTask);

        SelectDeviceCommand = new AsyncDelegateCommand<AttachedDevice>(async (device) =>
        {
            if (device != null && !(device.IsSelected && _deviceConnectionService.CanSend))
            {
                await _deviceConnectionService.SelectDeviceAsync(device);
            }
        });

        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        ObserveSelectedSnapshot();
        UpdateBar();
    }

    public AsyncDelegateCommand ActionCommand { get; }

    public AsyncDelegateCommand<AttachedDevice> SelectDeviceCommand { get; }

    public IReadOnlyList<IDac70Transport> Transports => _deviceConnectionService.Transports;

    public bool HasTransportChoice => Transports.Count > 1;

    public IDac70Transport? SelectedTransport
    {
        get => _deviceConnectionService.SelectedTransport;
        set
        {
            if (value != null && value != _deviceConnectionService.SelectedTransport)
            {
                _deviceConnectionService.SelectTransportAsync(value).SafeFireAndForget();
            }
        }
    }

    public ReadOnlyObservableCollection<AttachedDevice> Devices => _deviceConnectionService.Devices;

    public bool IsNumberShown => Devices.Count > 1;

    private bool _isPickerAvailable;
    public bool IsPickerAvailable
    {
        get => _isPickerAvailable;
        private set => SetProperty(ref _isPickerAvailable, value);
    }

    private bool _isPickerEnabled;
    public bool IsPickerEnabled
    {
        get => _isPickerEnabled;
        private set => SetProperty(ref _isPickerEnabled, value);
    }

    private bool _isChoiceRequired;
    public bool IsChoiceRequired
    {
        get => _isChoiceRequired;
        private set => SetProperty(ref _isChoiceRequired, value);
    }

    private DeviceStatusSeverity _severity;
    public DeviceStatusSeverity Severity
    {
        get => _severity;
        private set => SetProperty(ref _severity, value);
    }

    private LocalizedString _title = new(() => string.Empty);
    public LocalizedString Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    private LocalizedString _detail = new(() => string.Empty);
    public LocalizedString Detail
    {
        get => _detail;
        private set => SetProperty(ref _detail, value);
    }

    private LocalizedString? _actionText;
    public LocalizedString? ActionText
    {
        get => _actionText;
        private set => SetProperty(ref _actionText, value);
    }

    private bool _isProgressVisible;
    public bool IsProgressVisible
    {
        get => _isProgressVisible;
        private set => SetProperty(ref _isProgressVisible, value);
    }

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        private set => SetProperty(ref _progressValue, value);
    }

    private bool _isSyncInfoVisible;
    public bool IsSyncInfoVisible
    {
        get => _isSyncInfoVisible;
        private set => SetProperty(ref _isSyncInfoVisible, value);
    }

    private bool _isSyncWarning;
    public bool IsSyncWarning
    {
        get => _isSyncWarning;
        private set => SetProperty(ref _isSyncWarning, value);
    }

    private LocalizedString _syncText = new(() => string.Empty);
    public LocalizedString SyncText
    {
        get => _syncText;
        private set => SetProperty(ref _syncText, value);
    }

    private void OnDeviceConnectionServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceConnectionService.SelectedTransport))
        {
            RaisePropertyChanged(nameof(SelectedTransport));
        }

        if (e.PropertyName == nameof(DeviceConnectionService.SelectedDevice))
        {
            ObserveSelectedSnapshot();
        }

        if (e.PropertyName == nameof(DeviceConnectionService.Devices))
        {
            RaisePropertyChanged(nameof(Devices));
            RaisePropertyChanged(nameof(IsNumberShown));
        }

        UpdateBar();
    }

    private async Task ReconnectSelectedDeviceAsync()
    {
        if (_deviceConnectionService.SelectedDevice is { } device)
        {
            await _deviceConnectionService.SelectDeviceAsync(device);
        }
    }

    private void UpdateBar()
    {
        var devices = _deviceConnectionService.Devices;
        var selectedDevice = _deviceConnectionService.SelectedDevice;

        IsPickerAvailable = HasTransportChoice || devices.Count > (selectedDevice == null ? 0 : 1);
        IsPickerEnabled = _deviceConnectionService.State is not (DeviceState.Sending or DeviceState.Syncing);
        IsChoiceRequired = selectedDevice == null && devices.Count > 0;

        UpdateStatus(devices);
        UpdateSyncInfo();
    }

    private void ObserveSelectedSnapshot()
    {
        if (_observedSnapshot != null)
        {
            _observedSnapshot.PropertyChanged -= OnSnapshotPropertyChanged;
        }

        _observedSnapshot = _deviceConnectionService.SelectedDevice?.Snapshot;

        if (_observedSnapshot != null)
        {
            _observedSnapshot.PropertyChanged += OnSnapshotPropertyChanged;
        }
    }

    private void OnSnapshotPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceSnapshot.LastFullSync))
        {
            UpdateSyncInfo();
        }
    }

    private void UpdateSyncInfo()
    {
        var snapshot = _deviceConnectionService.SelectedDevice?.Snapshot;
        bool isSyncing = _deviceConnectionService.State == DeviceState.Syncing;

        IsSyncInfoVisible = snapshot != null;
        IsSyncWarning = snapshot?.LastFullSync == null && !isSyncing;

        if (isSyncing)
        {
            SyncText = new LocalizedString(() => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceSyncRunning)]);
        }
        else if (snapshot?.LastFullSync is { } lastFullSync)
        {
            SyncText = new LocalizedString(() => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceSyncedAt),
                lastFullSync.ToString(lastFullSync.Date == DateTime.Today ? "t" : "g", _localizationResourceManager.CurrentCulture)]);
        }
        else
        {
            SyncText = new LocalizedString(() => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceNotSynced)]);
        }
    }

    private void UpdateStatus(IReadOnlyList<AttachedDevice> devices)
    {
        var progress = _deviceConnectionService.State is DeviceState.Sending or DeviceState.Syncing
            ? _deviceConnectionService.Progress
            : null;

        IsProgressVisible = progress != null;
        ProgressValue = progress == null ? 0 : (double)progress.Completed / progress.Total;

        Func<string> title;
        if (IsNumberShown && _deviceConnectionService.SelectedDevice is { } selected)
        {
            title = () => $"{DeviceName} · {selected.Number} · {_deviceConnectionService.SelectedTransport?.DisplayName}";
        }
        else
        {
            title = () => $"{DeviceName} · {_deviceConnectionService.SelectedTransport?.DisplayName}";
        }
        Func<string> detail = () => string.Empty;

        switch (_deviceConnectionService.State)
        {
            case DeviceState.NoTransport:
                Severity = DeviceStatusSeverity.None;
                title = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceNoTransportTitle)];
                detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceNoTransportDetail)];
                ClearAction();
                break;
            case DeviceState.Disconnected:
                Severity = DeviceStatusSeverity.None;
                title = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceDisconnectedTitle)];
                detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceDisconnectedDetail)];
                ClearAction();
                break;
            case DeviceState.ChoiceRequired:
                Severity = DeviceStatusSeverity.Warning;
                title = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceChoiceTitle), devices.Count];
                detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceChoiceDetail)];
                ClearAction();
                break;
            case DeviceState.SelectionLost:
                Severity = DeviceStatusSeverity.Warning;
                title = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceLostTitle), DeviceName];
                detail = devices.Count > 0
                    ? () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceLostDetailOthers)]
                    : () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceLostDetailAlone)];
                ClearAction();
                break;
            case DeviceState.PermissionRequired:
                Severity = DeviceStatusSeverity.Warning;
                detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DevicePermissionDetail)];
                SetAction(ReconnectSelectedDeviceAsync, nameof(DeviceStatusBarSR.DeviceAllow));
                break;
            case DeviceState.Ready:
                Severity = DeviceStatusSeverity.Success;
                detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceReady)];
                ClearAction();
                break;
            case DeviceState.Sending:
                Severity = DeviceStatusSeverity.Info;

                if (progress != null)
                {
                    detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceSending),
                        progress.Completed + 1, progress.Total, progress.SettingName];
                }

                SetAction(_deviceConnectionService.CancelTransfer, nameof(DeviceStatusBarSR.DeviceCancel));
                break;
            case DeviceState.Syncing:
                Severity = DeviceStatusSeverity.Info;

                if (progress != null)
                {
                    detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceSyncing),
                        progress.Completed + 1, progress.Total, progress.SettingName];
                }

                SetAction(_deviceConnectionService.CancelTransfer, nameof(DeviceStatusBarSR.DeviceCancel));
                break;
            case DeviceState.SendCancelled:
                Severity = DeviceStatusSeverity.Warning;

                if (_deviceConnectionService.Progress is { } cancelled)
                {
                    detail = () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceSendCancelled),
                        cancelled.Completed, cancelled.Total];
                }

                ClearAction();
                break;
            case DeviceState.ConnectionFailed:
            case DeviceState.SendInterrupted:
                Severity = DeviceStatusSeverity.Error;
                detail = _deviceConnectionService.State == DeviceState.SendInterrupted && _deviceConnectionService.Progress is { } stopped
                    ? () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceFaultSendInterrupted), stopped.Completed, stopped.Total]
                    : () => _localizationResourceManager[nameof(DeviceStatusBarSR.DeviceFaultConnection)];

                if (_deviceConnectionService.SelectedDevice != null)
                {
                    SetAction(ReconnectSelectedDeviceAsync, nameof(DeviceStatusBarSR.DeviceReconnect));
                }
                else
                {
                    ClearAction();
                }

                break;
        }

        Title = new LocalizedString(title);
        Detail = new LocalizedString(detail);
    }

    private void SetAction(Action action, string textKey)
    {
        SetAction(() =>
        {
            action();
            return Task.CompletedTask;
        }, textKey);
    }

    private void SetAction(Func<Task> action, string textKey)
    {
        _action = action;
        ActionText = new LocalizedString(() => _localizationResourceManager[textKey]);
    }

    private void ClearAction()
    {
        _action = null;
        ActionText = null;
    }

    public void Dispose()
    {
        _deviceConnectionService.PropertyChanged -= OnDeviceConnectionServiceChanged;

        if (_observedSnapshot != null)
        {
            _observedSnapshot.PropertyChanged -= OnSnapshotPropertyChanged;
            _observedSnapshot = null;
        }
    }
}
