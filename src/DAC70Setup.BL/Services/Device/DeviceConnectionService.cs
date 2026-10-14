using System.Collections.ObjectModel;
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

    private AttachedDevice? _lostDevice;

    public DeviceConnectionService(IEnumerable<IDac70Transport> transports)
    {
        Transports = transports.Where(transport => transport.IsSupported).ToList();
        Devices = new ReadOnlyObservableCollection<AttachedDevice>(_devices);

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

    private readonly ObservableCollection<AttachedDevice> _devices = [];
    public ReadOnlyObservableCollection<AttachedDevice> Devices { get; }

    private AttachedDevice? _selectedDevice;
    public AttachedDevice? SelectedDevice
    {
        get => _selectedDevice;
        private set
        {
            var previous = _selectedDevice;

            if (SetProperty(ref _selectedDevice, value))
            {
                if (previous != null)
                {
                    previous.IsSelected = false;
                }

                if (value != null)
                {
                    value.IsSelected = true;
                }
            }
        }
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

            SelectedDevice = null;
            _lostDevice = null;
            _devices.Clear();
            Progress = null;
            State = DeviceState.Disconnected;

            await RefreshDevicesAsyncInt(transport);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SelectDeviceAsync(AttachedDevice device)
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
                await SendConfigurationAsyncInt(transport, device.Device, values);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task RefreshDevicesAsyncInt(IDac70Transport transport)
    {
        UpdateDevices(await transport.DiscoverAsync(CancellationToken.None).ToListAsync());

        if (SelectedDevice != null)
        {
            var sameDevice = _devices.FirstOrDefault(row => IsSameDevice(row.Device, SelectedDevice.Device));
            if (sameDevice != null)
            {
                SelectedDevice = sameDevice;
                return;
            }

            _lostDevice = SelectedDevice;
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
            var returnedDevice = _devices.FirstOrDefault(row => IsSameDevice(row.Device, _lostDevice.Device));
            if (returnedDevice != null)
            {
                await SelectDeviceAsyncInt(transport, returnedDevice);
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

    private async Task SelectDeviceAsyncInt(IDac70Transport transport, AttachedDevice device)
    {
        // A selection queued behind a transport switch or devices refresh can carry a row that is gone.
        var currentDevice = _devices.FirstOrDefault(row => IsSameDevice(row.Device, device.Device));
        if (currentDevice == null)
        {
            return;
        }

        DeviceState state;

        try
        {
            var connection = await transport.ConnectAsync(currentDevice.Device, CancellationToken.None);
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

    private void UpdateDevices(IReadOnlyList<Dac70Device> discovered)
    {
        var reserved = _lostDevice ?? SelectedDevice;

        for (int i = _devices.Count - 1; i >= 0; i--)
        {
            if (!discovered.Any(device => IsSameConnection(_devices[i].Device, device)))
            {
                _devices.RemoveAt(i);
            }
        }

        foreach (var device in discovered)
        {
            if (_devices.FirstOrDefault(row => IsSameConnection(row.Device, device)) is { } existing)
            {
                existing.Device = device;
                continue;
            }

            int number = reserved != null && IsSameDevice(reserved.Device, device) ? reserved.Number : GetFreeNumber(reserved);
            int index = _devices.TakeWhile(row => row.Number < number).Count();
            _devices.Insert(index, new AttachedDevice(device, number));
        }

        RaisePropertyChanged(nameof(Devices));
    }

    private int GetFreeNumber(AttachedDevice? reserved)
    {
        int number = 1;

        while (_devices.Any(row => row.Number == number) || reserved?.Number == number)
        {
            number++;
        }

        return number;
    }

    private static bool IsSameConnection(Dac70Device first, Dac70Device second)
    {
        return first.Id == second.Id
            && (first.SerialNumber == null || second.SerialNumber == null || first.SerialNumber == second.SerialNumber);
    }

    private static bool IsSameDevice(Dac70Device first, Dac70Device second)
    {
        return first.SerialNumber != null && second.SerialNumber != null
            ? first.SerialNumber == second.SerialNumber
            : first.Id == second.Id;
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => _ = RefreshDevicesAsync());
    }
}
