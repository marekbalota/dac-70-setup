using DAC70Setup.BL.Models;
using DAC70Setup.Core.Exceptions;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.Device;

public class DeviceConnectionService : BindableBase
{
    private const string SelectedTransportIdKey = "SelectedTransportId";

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private CancellationTokenSource? _transferCancellation;

    private Dac70Device? _lostDevice;
    private bool _lostDeviceWasAlone;

    public DeviceConnectionService(IEnumerable<IDac70Transport> transports)
    {
        Transports = transports.Where(transport => transport.IsSupported).ToList();

        string savedId = Preferences.Default.Get(SelectedTransportIdKey, string.Empty);
        _selectedTransport = Transports.FirstOrDefault(transport => transport.Id == savedId)
            ?? Transports.FirstOrDefault();

        if (_selectedTransport != null)
        {
            _selectedTransport.DevicesChanged += OnDevicesChanged;
        }

        _state = _selectedTransport == null ? DeviceState.NoTransport : DeviceState.Disconnected;
    }

    public IReadOnlyList<IDac70Transport> Transports { get; }

    private IDac70Transport? _selectedTransport;
    public IDac70Transport? SelectedTransport
    {
        get => _selectedTransport;
        private set => SetProperty(ref _selectedTransport, value);
    }

    private IReadOnlyList<Dac70Device> _devices = [];
    public IReadOnlyList<Dac70Device> Devices
    {
        get => _devices;
        private set => SetProperty(ref _devices, value);
    }

    private Dac70Device? _selectedDevice;
    public Dac70Device? SelectedDevice
    {
        get => _selectedDevice;
        private set => SetProperty(ref _selectedDevice, value);
    }

    private DeviceState _state;
    public DeviceState State
    {
        get => _state;
        private set => SetProperty(ref _state, value, () => RaisePropertyChanged(nameof(CanSend)));
    }

    public bool CanSend => State is DeviceState.Ready or DeviceState.SendCancelled;

    private TransferProgress? _progress;
    public TransferProgress? Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    public async Task RefreshDevicesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (SelectedTransport is { } transport)
            {
                await RefreshDevicesAsyncInt(transport);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SelectTransportAsync(IDac70Transport transport)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (transport == SelectedTransport)
            {
                return;
            }

            if (SelectedTransport != null)
            {
                SelectedTransport.DevicesChanged -= OnDevicesChanged;
            }

            SelectedTransport = transport;
            transport.DevicesChanged += OnDevicesChanged;
            Preferences.Default.Set(SelectedTransportIdKey, transport.Id);

            Devices = [];
            SelectedDevice = null;
            _lostDevice = null;
            _lostDeviceWasAlone = false;
            Progress = null;
            State = DeviceState.Disconnected;

            await RefreshDevicesAsyncInt(transport);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SelectDeviceAsync(Dac70Device device)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (SelectedTransport is { } transport)
            {
                await SelectDeviceAsyncInt(transport, device);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SendConfigurationAsync(Configuration configuration)
    {
        var values = configuration.Settings.ToList();

        await _semaphore.WaitAsync();
        try
        {
            if (SelectedTransport is { } transport && SelectedDevice is { } device
                && CanSend && values.Count > 0)
            {
                await SendConfigurationAsyncInt(transport, device, values);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task RefreshDevicesAsyncInt(IDac70Transport transport)
    {
        Devices = await transport.DiscoverAsync(CancellationToken.None).ToListAsync();

        if (SelectedDevice != null)
        {
            var sameDevice = FindSameDevice(SelectedDevice);

            if (sameDevice != null)
            {
                SelectedDevice = sameDevice;
                return;
            }

            _lostDevice = SelectedDevice;
            _lostDeviceWasAlone = Devices.Count == 0;
            SelectedDevice = null;

            if (State != DeviceState.SendInterrupted)
            {
                Progress = null;
                State = DeviceState.SelectionLost;
            }

            return;
        }

        if (_lostDevice != null)
        {
            var returnedDevice = FindSameDevice(_lostDevice);

            if (returnedDevice != null)
            {
                await SelectDeviceAsyncInt(transport, returnedDevice);
            }
            else if (_lostDeviceWasAlone && Devices.Count == 1)
            {
                await SelectDeviceAsyncInt(transport, Devices[0]);
            }

            return;
        }

        if (Devices.Count == 1)
        {
            await SelectDeviceAsyncInt(transport, Devices[0]);
        }
        else if (Devices.Count > 1)
        {
            State = DeviceState.ChoiceRequired;
        }
        else
        {
            State = DeviceState.Disconnected;
        }
    }

    private async Task SendConfigurationAsyncInt(IDac70Transport transport, Dac70Device device, List<Dac70SettingValue> values)
    {
        using var transferCancellation = new CancellationTokenSource();
        _transferCancellation = transferCancellation;

        IDac70Connection? connection = null;

        try
        {
            Progress = new TransferProgress(0, values.Count, values[0].Definition.Name);
            State = DeviceState.Sending;

            try
            {
                connection = await transport.ConnectAsync(device, transferCancellation.Token);
            }
            catch (Dac70PermissionDeniedException)
            {
                State = DeviceState.PermissionRequired;
                return;
            }
            catch (IOException)
            {
                State = DeviceState.ConnectionFailed;
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                Progress = new TransferProgress(i, values.Count, values[i].Definition.Name);
                await connection.WriteReportAsync(values[i].GetPayload(), transferCancellation.Token);
            }
        }
        catch (OperationCanceledException)
        {
            if (Progress?.Completed > 0)
            {
                State = DeviceState.SendCancelled;
        }
            }
        catch (IOException)
        {
            State = DeviceState.SendInterrupted;
        }
        finally
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            _transferCancellation = null;

            if (State == DeviceState.Sending)
            {
                State = DeviceState.Ready;
            }

            if (State is not (DeviceState.SendInterrupted or DeviceState.SendCancelled))
            {
                Progress = null;
            }
        }
    }

    private async Task SelectDeviceAsyncInt(IDac70Transport transport, Dac70Device device)
    {
        // A selection queued behind a transport switch or devices refresh can carry a stale record.
        var currentDevice = FindSameDevice(device);
        if (currentDevice == null)
        {
            return;
        }

        DeviceState state;

        try
        {
            var connection = await transport.ConnectAsync(currentDevice, CancellationToken.None);
            await connection.DisposeAsync();

            state = DeviceState.Ready;
        }
        catch (Dac70PermissionDeniedException)
        {
            state = DeviceState.PermissionRequired;
        }
        catch (IOException)
        {
            state = DeviceState.ConnectionFailed;
        }

        SelectedDevice = currentDevice;
        _lostDevice = null;
        Progress = null;
        State = state;
    }

    public void CancelTransfer()
    {
        _transferCancellation?.Cancel();
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => _ = RefreshDevicesAsync());
    }

    private Dac70Device? FindSameDevice(Dac70Device device)
    {
        return Devices.FirstOrDefault(candidate => candidate.Id == device.Id)
            ?? Devices.FirstOrDefault(candidate => device.SerialNumber != null && candidate.SerialNumber == device.SerialNumber);
    }
}
