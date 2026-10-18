using System.Collections.ObjectModel;
using DAC70Setup.BL.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.Core.Exceptions;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.Device;

public partial class DeviceConnectionService : ObservableObject
{
    private const string SelectedTransportIdKey = "SelectedTransportId";

    private const int ReplyTimeoutMilliseconds = 3000;
    private const int SkippedReports = 10;
    private const int ComparedReplyBytes = 8;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private CancellationTokenSource? _transferCancellation;

    public DeviceConnectionService(IEnumerable<IDac70Transport> transports)
    {
        Transports = transports.Where(transport => transport.IsSupported).ToList();
        Devices = new ReadOnlyObservableCollection<AttachedDevice>(_devices);

        string savedId = Preferences.Default.Get(SelectedTransportIdKey, string.Empty);
        SelectedTransport = Transports.FirstOrDefault(transport => transport.Id == savedId)
            ?? Transports.FirstOrDefault();

        if (SelectedTransport != null)
        {
            SelectedTransport.DevicesChanged += OnDevicesChanged;
        }

        State = SelectedTransport == null ? DeviceState.NoTransport : DeviceState.Disconnected;
    }

    public IReadOnlyList<IDac70Transport> Transports { get; }

    [ObservableProperty]
    public partial IDac70Transport? SelectedTransport { get; private set; }

    private readonly ObservableCollection<AttachedDevice> _devices = [];
    public ReadOnlyObservableCollection<AttachedDevice> Devices { get; }

    [ObservableProperty]
    public partial AttachedDevice? SelectedDevice { get; private set; }

    partial void OnSelectedDeviceChanged(AttachedDevice? oldValue, AttachedDevice? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
        }

        if (newValue != null)
        {
            newValue.IsSelected = true;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSend))]
    public partial DeviceState State { get; private set; }

    public bool CanSend => State is DeviceState.Ready or DeviceState.SendCancelled;

    [ObservableProperty]
    public partial TransferProgress? Progress { get; private set; }

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

    public Task SendConfigurationAsync(Configuration configuration)
    {
        return SendValuesAsync(configuration.Settings.ToList());
    }

    public async Task SendValuesAsync(IReadOnlyList<Dac70SettingValue> values)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (SelectedTransport is { } transport && SelectedDevice is { } device
                && CanSend && values.Count > 0)
            {
                await SendValuesAsyncInt(transport, device, values);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <returns>True when every readable setting was read, false when the sync did not run or stopped early.</returns>
    public async Task<bool> SyncAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (SelectedTransport is { } transport && SelectedDevice is { } device && CanSend)
            {
                return await SyncAsyncInt(transport, device);
            }

            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task RefreshDevicesAsyncInt(IDac70Transport transport)
    {
        bool isDeviceAdded = UpdateDevices(await transport.DiscoverAsync(CancellationToken.None).ToListAsync());

        if (SelectedDevice != null)
        {
            if (_devices.Contains(SelectedDevice))
            {
                return;
            }

            SelectedDevice = null;

            if (State != DeviceState.SendInterrupted)
            {
                Progress = null;
                State = DeviceState.SelectionLost;
            }

            return;
        }

        if (State is DeviceState.SelectionLost or DeviceState.SendInterrupted && !isDeviceAdded)
        {
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

    private async Task SendValuesAsyncInt(IDac70Transport transport, AttachedDevice device, IReadOnlyList<Dac70SettingValue> values)
    {
        using var transferCancellation = new CancellationTokenSource();
        _transferCancellation = transferCancellation;

        var snapshot = device.Snapshot;
        IDac70Connection? connection = null;

        try
        {
            Progress = new TransferProgress(0, values.Count, values[0].Definition.Name);
            State = DeviceState.Sending;

            try
            {
                connection = await transport.ConnectAsync(device.Device, transferCancellation.Token);
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
                snapshot.SetValue(values[i]);
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

    private async Task<bool> SyncAsyncInt(IDac70Transport transport, AttachedDevice device)
    {
        var requests = Dac70SettingCatalog.All
            .Where(definition => definition.IsReadable)
            .GroupBy(definition => (definition.Key.Type, definition.ReadRequestId))
            .Select(group => group.ToList())
            .ToList();
        int total = requests.Sum(request => request.Count);
        var snapshot = device.Snapshot;

        using var transferCancellation = new CancellationTokenSource();
        _transferCancellation = transferCancellation;

        IDac70Connection? connection = null;
        bool isComplete = false;

        try
        {
            Progress = new TransferProgress(0, total, requests[0][0].Name);
            State = DeviceState.Syncing;

            try
            {
                connection = await transport.ConnectAsync(device.Device, transferCancellation.Token);
            }
            catch (Dac70PermissionDeniedException)
            {
                State = DeviceState.PermissionRequired;
                return false;
            }
            catch (IOException)
            {
                State = DeviceState.ConnectionFailed;
                return false;
            }

            byte[]? previousReply = null;
            int completed = 0;

            foreach (var request in requests)
            {
                Progress = new TransferProgress(completed, total, request[0].Name);
                await connection.WriteReportAsync(request[0].GetReadPayload(), transferCancellation.Token);

                var reply = await ReadReplyAsync(connection, previousReply, transferCancellation.Token);
                completed += request.Count;

                if (reply == null)
                {
                    continue;
                }

                previousReply = reply;

                foreach (var definition in request)
                {
                    if (definition.CreateValueFromReadResponse(reply) is { } value)
                    {
                        snapshot.SetValue(value);
                    }
                }
            }

            snapshot.CompleteFullSync(DateTime.Now);
            isComplete = true;
        }
        catch (OperationCanceledException)
        {
            // Values read so far stay in the snapshot
        }
        catch (IOException)
        {
            State = DeviceState.ConnectionFailed;
        }
        finally
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            _transferCancellation = null;

            if (State == DeviceState.Syncing)
            {
                State = DeviceState.Ready;
            }

            Progress = null;
        }

        return isComplete;
    }

    private static async Task<byte[]?> ReadReplyAsync(IDac70Connection connection, byte[]? previousReply, CancellationToken cancellationToken)
    {
        using var replyCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        replyCancellation.CancelAfter(ReplyTimeoutMilliseconds);

        byte[]? reply = null;
        int count = 0;

        try
        {
            while (true)
            {
                var report = await connection.ReadReportAsync(replyCancellation.Token);
                count++;

                if (previousReply == null && count < SkippedReports)
                {
                    continue;
                }

                if (previousReply != null && report.AsSpan(0, ComparedReplyBytes).SequenceEqual(previousReply.AsSpan(0, ComparedReplyBytes)))
                {
                    continue;
                }

                previousReply = report;
                reply = report;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The reply timeout exceeded
        }

        return reply;
    }

    private async Task SelectDeviceAsyncInt(IDac70Transport transport, AttachedDevice device)
    {
        // A selection queued behind a transport switch or devices refresh can carry a row that is gone.
        if (!_devices.Contains(device))
        {
            return;
        }

        DeviceState state;

        try
        {
            var connection = await transport.ConnectAsync(device.Device, CancellationToken.None);
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

        SelectedDevice = device;
        Progress = null;
        State = state;
    }

    public void CancelTransfer()
    {
        _transferCancellation?.Cancel();
    }

    private bool UpdateDevices(IReadOnlyList<Dac70Device> discovered)
    {
        bool isDeviceAdded = false;

        for (int i = _devices.Count - 1; i >= 0; i--)
        {
            if (!discovered.Any(device => device.Id == _devices[i].Device.Id))
            {
                _devices.RemoveAt(i);
            }
        }

        foreach (var device in discovered)
        {
            if (_devices.FirstOrDefault(row => row.Device.Id == device.Id) is { } existing)
            {
                existing.Device = device;
                continue;
            }

            int number = GetFreeNumber();
            int index = _devices.TakeWhile(row => row.Number < number).Count();
            _devices.Insert(index, new AttachedDevice(device, number));
            isDeviceAdded = true;
        }

        OnPropertyChanged(nameof(Devices));

        return isDeviceAdded;
    }

    private int GetFreeNumber()
    {
        int number = 1;

        while (_devices.Any(row => row.Number == number))
        {
            number++;
        }

        return number;
    }

    private void OnDevicesChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => _ = RefreshDevicesAsync());
    }
}
