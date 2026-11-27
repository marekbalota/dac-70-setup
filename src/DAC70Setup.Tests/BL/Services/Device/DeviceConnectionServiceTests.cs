using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Exceptions;
using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;
using DAC70Setup.Tests.Fakes;

namespace DAC70Setup.Tests.BL.Services.Device;

public class DeviceConnectionServiceTests
{
    private static readonly byte[] FirmwareReply = Report.Create(0x13, 0x3E, 0x34, 0x12);
    private static readonly byte[] InputSignalReply = Report.Create(0x13, 0x3E, 0x07, 0x80, 0x04, 0x38, 59, 94, 0x01);
    private static readonly int ReadableSettingCount = Dac70SettingCatalog.All.Count(definition => definition.IsReadable);

    private readonly FakeDac70Transport _transport = new();
    private readonly FakePreferences _preferences = new();
    private readonly FakeDispatcher _dispatcher = new();
    private readonly List<Dac70SettingValue> _values = CreateValues();

    [Fact]
    public void Constructor_OnlyUnsupportedTransports_HasNoTransport()
    {
        var service = CreateService(new FakeDac70Transport { IsSupported = false });

        Assert.Null(service.SelectedTransport);
        Assert.Equal(DeviceState.NoTransport, service.State);
    }

    [Fact]
    public async Task SelectTransportAsync_IsRememberedForTheNextStart()
    {
        var other = new FakeDac70Transport("other");
        var service = CreateService(_transport, other);

        await service.SelectTransportAsync(other);
        var restarted = CreateService(_transport, other);

        Assert.Same(other, restarted.SelectedTransport);
    }

    [Fact]
    public async Task SelectTransportAsync_ShowsTheDevicesOfTheNewTransport()
    {
        _transport.Attach("device-1");
        var other = new FakeDac70Transport("other");
        other.Attach("device-2");
        var service = CreateService(_transport, other);
        await service.RefreshDevicesAsync();

        Assert.Equal("device-1", Assert.Single(service.Devices).Device.Id);
        Assert.Equal("device-1", service.SelectedDevice?.Device.Id);

        await service.SelectTransportAsync(other);

        Assert.Equal("device-2", Assert.Single(service.Devices).Device.Id);
        Assert.Equal("device-2", service.SelectedDevice?.Device.Id);
    }

    [Fact]
    public async Task RefreshDevicesAsync_OneDevice_SelectsIt()
    {
        _transport.Attach("device-1");
        var service = CreateService();

        await service.RefreshDevicesAsync();

        Assert.Equal("device-1", service.SelectedDevice?.Device.Id);
        Assert.Equal(DeviceState.Ready, service.State);
    }

    [Fact]
    public async Task RefreshDevicesAsync_TwoDevices_LetsTheUserChoose()
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        var service = CreateService();

        await service.RefreshDevicesAsync();

        Assert.Null(service.SelectedDevice);
        Assert.Equal(DeviceState.ChoiceRequired, service.State);
    }

    // Switching to the remaining device unasked could send a configuration to the wrong one,
    // and unplugging can raise more than one change, so the device list is refreshed twice
    [Fact]
    public async Task RefreshDevicesAsync_SelectedDeviceUnplugged_DoesNotSwitchToTheOtherOne()
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        var service = CreateService();
        await service.RefreshDevicesAsync();
        await service.SelectDeviceAsync(service.Devices.Single(row => row.Device.Id == "device-1"));

        _transport.Detach("device-1");
        await service.RefreshDevicesAsync();
        await service.RefreshDevicesAsync();

        Assert.Null(service.SelectedDevice);
        Assert.Equal(DeviceState.SelectionLost, service.State);
    }

    [Fact]
    public async Task RefreshDevicesAsync_SelectionLostThenOneDevicePluggedIn_SelectsIt()
    {
        _transport.Attach("device-1");
        var service = CreateService();
        await service.RefreshDevicesAsync();
        _transport.Detach("device-1");
        await service.RefreshDevicesAsync();
        Assert.Equal(DeviceState.SelectionLost, service.State);

        _transport.Attach("device-2");
        await service.RefreshDevicesAsync();

        Assert.Equal("device-2", service.SelectedDevice?.Device.Id);
        Assert.Equal(DeviceState.Ready, service.State);
    }

    [Fact]
    public async Task RefreshDevicesAsync_NewDevice_TakesTheLowestFreeNumber()
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        _transport.Attach("device-3");
        var service = CreateService();
        await service.RefreshDevicesAsync();

        _transport.Detach("device-1");
        _transport.Attach("device-4");
        await service.RefreshDevicesAsync();

        (int, string)[] expected = [(1, "device-4"), (2, "device-2"), (3, "device-3")];
        Assert.Equal(expected, service.Devices.Select(row => (row.Number, row.Device.Id)));
    }

    [Theory]
    [InlineData(typeof(IOException), DeviceState.ConnectionFailed)]
    [InlineData(typeof(Dac70PermissionDeniedException), DeviceState.PermissionRequired)]
    public async Task SelectDeviceAsync_ConnectFails_SelectsItAndReportsWhy(Type exceptionType, DeviceState expectedState)
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        var service = CreateService();
        await service.RefreshDevicesAsync();
        _transport.ConnectException = (Exception)Activator.CreateInstance(exceptionType)!;

        await service.SelectDeviceAsync(service.Devices[0]);

        Assert.Same(service.Devices[0], service.SelectedDevice);
        Assert.Equal(expectedState, service.State);
    }

    [Fact]
    public async Task SelectDeviceAsync_OtherDevice_MovesTheSelectionMark()
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        var service = CreateService();
        await service.RefreshDevicesAsync();
        await service.SelectDeviceAsync(service.Devices[0]);
        Assert.True(service.Devices[0].IsSelected);
        Assert.False(service.Devices[1].IsSelected);

        await service.SelectDeviceAsync(service.Devices[1]);

        Assert.False(service.Devices[0].IsSelected);
        Assert.True(service.Devices[1].IsSelected);
    }

    // A selection queued behind a refresh can carry a row that the refresh removed
    [Fact]
    public async Task SelectDeviceAsync_DeviceNoLongerListed_IsIgnored()
    {
        _transport.Attach("device-1");
        _transport.Attach("device-2");
        _transport.Attach("device-3");
        var service = CreateService();
        await service.RefreshDevicesAsync();
        var removed = service.Devices.Single(row => row.Device.Id == "device-3");
        _transport.Detach("device-3");
        await service.RefreshDevicesAsync();

        await service.SelectDeviceAsync(removed);

        Assert.Null(service.SelectedDevice);
        Assert.Equal(DeviceState.ChoiceRequired, service.State);
    }

    // USB raises the event on a background thread, so the refresh must wait for the UI thread
    [Fact]
    public void DevicesChanged_RefreshesTheDevicesOnTheUiThread()
    {
        var service = CreateService();
        _transport.Attach("device-1");

        _transport.RaiseDevicesChanged();

        Assert.Empty(service.Devices);
        _dispatcher.RunQueued();
        Assert.Equal("device-1", Assert.Single(service.Devices).Device.Id);
    }

    [Fact]
    public async Task SendValuesAsync_WritesEveryValueInOrderAndRemembersThem()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();

        await service.SendValuesAsync(_values);

        Assert.Equal(_values.Select(value => value.GetPayload()), device.WrittenReports);
        var snapshot = service.SelectedDevice!.Snapshot;
        Assert.Equal(_values.Select(value => value.DisplayValueText),
            _values.Select(value => snapshot.Values[value.Definition.Key].DisplayValueText));
        Assert.Equal(DeviceState.Ready, service.State);
        Assert.Null(service.Progress);
    }

    [Fact]
    public async Task SendValuesAsync_NoSelectedDevice_SendsNothing()
    {
        var device = _transport.Attach("device-1");
        _transport.Attach("device-2");
        var service = CreateService();
        await service.RefreshDevicesAsync();

        await service.SendValuesAsync(_values);

        Assert.Empty(device.WrittenReports);
        Assert.Equal(DeviceState.ChoiceRequired, service.State);
    }

    [Theory]
    [InlineData(typeof(IOException), DeviceState.ConnectionFailed)]
    [InlineData(typeof(Dac70PermissionDeniedException), DeviceState.PermissionRequired)]
    public async Task SendValuesAsync_ConnectFails_ReportsWhy(Type exceptionType, DeviceState expectedState)
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        _transport.ConnectException = (Exception)Activator.CreateInstance(exceptionType)!;

        await service.SendValuesAsync(_values);

        Assert.Empty(device.WrittenReports);
        Assert.Equal(expectedState, service.State);
        Assert.Null(service.Progress);
    }

    // The progress stays so the user can see how far the send got
    [Fact]
    public async Task SendValuesAsync_WriteFailsPartway_KeepsProgressAndOnlyTheSentValues()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = written =>
        {
            if (written == 1)
            {
                throw new IOException();
            }
        };

        await service.SendValuesAsync(_values);

        Assert.Equal(DeviceState.SendInterrupted, service.State);
        Assert.Equal(new TransferProgress(1, 3, _values[1].Definition.Name), service.Progress);
        Assert.Equal([_values[0].Definition.Key], service.SelectedDevice!.Snapshot.Values.Keys);
    }

    [Fact]
    public async Task RefreshDevicesAsync_DeviceUnpluggedAfterInterruptedSend_StaysInterrupted()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = written =>
        {
            if (written == 1)
            {
                throw new IOException();
            }
        };
        await service.SendValuesAsync(_values);
        Assert.Equal(DeviceState.SendInterrupted, service.State);

        _transport.Detach("device-1");
        await service.RefreshDevicesAsync();

        Assert.Null(service.SelectedDevice);
        Assert.Equal(DeviceState.SendInterrupted, service.State);
        Assert.NotNull(service.Progress);
    }

    [Fact]
    public async Task SendValuesAsync_CancelledAfterFirstValue_KeepsProgressAndAllowsSendingAgain()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = written =>
        {
            if (written == 1)
            {
                service.CancelTransfer();
            }
        };

        await service.SendValuesAsync(_values);

        Assert.Single(device.WrittenReports);
        Assert.Equal(DeviceState.SendCancelled, service.State);
        Assert.Equal(new TransferProgress(1, 3, _values[1].Definition.Name), service.Progress);
        Assert.True(service.CanSend);
    }

    [Fact]
    public async Task SendValuesAsync_CancelledBeforeFirstValue_IsReadyAgain()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = _ => service.CancelTransfer();

        await service.SendValuesAsync(_values);

        Assert.Empty(device.WrittenReports);
        Assert.Equal(DeviceState.Ready, service.State);
        Assert.Null(service.Progress);
    }

    [Fact]
    public async Task FactoryResetAsync_WritesTheResetReport()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();

        await service.FactoryResetAsync();

        Assert.Equal(Dac70FactoryReset.GetPayload(), Assert.Single(device.WrittenReports));
        Assert.Equal(DeviceState.Ready, service.State);
    }

    [Fact]
    public async Task FactoryResetAsync_AfterDownload_ClearsTheDownloadedValues()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        AnswerDownloadReads(device);
        Assert.True(await service.DownloadAsync());
        var snapshot = service.SelectedDevice!.Snapshot;
        Assert.NotEmpty(snapshot.Values);
        Assert.NotNull(snapshot.LastDownload);

        await service.FactoryResetAsync();

        Assert.Empty(snapshot.Values);
        Assert.Null(snapshot.LastDownload);
        Assert.NotNull(snapshot.FirmwareVersion);
    }

    [Theory]
    [InlineData(typeof(IOException), DeviceState.ConnectionFailed)]
    [InlineData(typeof(Dac70PermissionDeniedException), DeviceState.PermissionRequired)]
    public async Task FactoryResetAsync_ConnectFails_ReportsWhy(Type exceptionType, DeviceState expectedState)
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        _transport.ConnectException = (Exception)Activator.CreateInstance(exceptionType)!;

        await service.FactoryResetAsync();

        Assert.Empty(device.WrittenReports);
        Assert.Equal(expectedState, service.State);
    }

    [Fact]
    public async Task FactoryResetAsync_WriteFails_ConnectionFailedAndKeepsTheValues()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        await service.SendValuesAsync(_values);
        device.BeforeWrite = _ => throw new IOException();

        await service.FactoryResetAsync();

        Assert.Equal(DeviceState.ConnectionFailed, service.State);
        Assert.Equal(_values.Count, service.SelectedDevice!.Snapshot.Values.Count);
    }

    [Fact]
    public async Task DownloadAsync_ReadsEverySettingTheFirmwareAndTheInputSignal()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        AnswerDownloadReads(device);
        var inputSource = Catalog.Get<EnumSettingDefinition>("Input Source");
        var brightness = Catalog.Get<RangeSettingDefinition>("Brightness");

        bool isComplete = await service.DownloadAsync();

        var snapshot = service.SelectedDevice!.Snapshot;
        Assert.True(isComplete);
        Assert.Equal(ReadableSettingCount, snapshot.Values.Count);
        Assert.Contains(inputSource.Key, snapshot.Values.Keys);
        Assert.Contains(brightness.Key, snapshot.Values.Keys);
        // The four Proc Amp settings share one request
        Assert.Single(device.WrittenReports, report => report.SequenceEqual(brightness.GetReadPayload()));
        Assert.Equal(Dac70FirmwareVersion.FromReadResponse(FirmwareReply), snapshot.FirmwareVersion);
        Assert.Equal(Dac70InputSignal.FromReadResponse(InputSignalReply), snapshot.InputSignal);
        Assert.NotNull(snapshot.LastDownload);
        Assert.Equal(DeviceState.Ready, service.State);
        Assert.Null(service.Progress);
    }

    // Takes 6 s: the setting without an answer waits out the session's reply timeout
    [Fact]
    public async Task DownloadAsync_SettingWithoutReply_IsIncompleteButKeepsTheRest()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        var inputSource = Catalog.Get<EnumSettingDefinition>("Input Source");
        AnswerDownloadReads(device, unanswered: inputSource);

        bool isComplete = await service.DownloadAsync();

        var snapshot = service.SelectedDevice!.Snapshot;
        Assert.False(isComplete);
        Assert.Null(snapshot.LastDownload);
        Assert.Equal(ReadableSettingCount - 1, snapshot.Values.Count);
        Assert.DoesNotContain(inputSource.Key, snapshot.Values.Keys);
        Assert.Equal(DeviceState.Ready, service.State);
    }

    [Fact]
    public async Task DownloadAsync_CancelledPartway_ReturnsFalseAndKeepsTheValuesReadSoFar()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        AnswerDownloadReads(device);
        device.BeforeWrite = _ =>
        {
            if (service.SelectedDevice!.Snapshot.Values.Count > 0)
            {
                service.CancelTransfer();
            }
        };

        bool isComplete = await service.DownloadAsync();

        var snapshot = service.SelectedDevice!.Snapshot;
        Assert.False(isComplete);
        Assert.Single(snapshot.Values);
        Assert.Null(snapshot.LastDownload);
        Assert.Equal(DeviceState.Ready, service.State);
        Assert.Null(service.Progress);
    }

    [Fact]
    public async Task DownloadAsync_WriteFails_ReturnsFalseAndConnectionFailed()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = _ => throw new IOException();

        bool isComplete = await service.DownloadAsync();

        Assert.False(isComplete);
        Assert.Equal(DeviceState.ConnectionFailed, service.State);
        Assert.Null(service.Progress);
    }

    [Theory]
    [InlineData(typeof(IOException), DeviceState.ConnectionFailed)]
    [InlineData(typeof(Dac70PermissionDeniedException), DeviceState.PermissionRequired)]
    public async Task DownloadAsync_ConnectFails_ReturnsFalseAndReportsWhy(Type exceptionType, DeviceState expectedState)
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        _transport.ConnectException = (Exception)Activator.CreateInstance(exceptionType)!;

        bool isComplete = await service.DownloadAsync();

        Assert.False(isComplete);
        Assert.Empty(device.WrittenReports);
        Assert.Equal(expectedState, service.State);
    }

    private DeviceConnectionService CreateService(params IDac70Transport[] transports)
    {
        return new DeviceConnectionService(transports.Length > 0 ? transports : [_transport], _preferences, _dispatcher);
    }

    // One device, selected and ready
    private async Task<(DeviceConnectionService Service, FakeDac70Connection Device)> CreateServiceWithDeviceAsync()
    {
        var device = _transport.Attach("device-1");
        var service = CreateService();
        await service.RefreshDevicesAsync();

        return (service, device);
    }

    private static List<Dac70SettingValue> CreateValues()
    {
        var inputSource = Catalog.Get<EnumSettingDefinition>("Input Source");
        var audioSource = Catalog.Get<EnumSettingDefinition>("Audio Source");

        return
        [
            new EnumSettingValue(inputSource) { SelectedOption = Catalog.GetOption(inputSource, "SDI") },
            new EnumSettingValue(audioSource) { SelectedOption = Catalog.GetOption(audioSource, "L/R Audio") },
            new RangeSettingValue(Catalog.Get<RangeSettingDefinition>("Brightness")) { Value = 0x90 }
        ];
    }

    private static void AnswerDownloadReads(FakeDac70Connection device, Dac70SettingDefinition? unanswered = null)
    {
        device.Answer(Dac70FirmwareVersion.GetReadPayload(), FirmwareReply);
        device.Answer(Dac70InputSignal.GetReadPayload(), InputSignalReply);

        foreach (var definition in Dac70SettingCatalog.All.Where(definition => definition.IsReadable && definition != unanswered))
        {
            byte value = definition is EnumSettingDefinition enumDefinition ? enumDefinition.Options[0].Value : (byte)0;
            device.Answer(definition.GetReadPayload(), Report.Create(0x13, 0x3E, value));
        }
    }
}