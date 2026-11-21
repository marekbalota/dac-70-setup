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
    private readonly FakeDac70Transport _transport = new();
    private readonly FakePreferences _preferences = new();
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

    [Fact]
    public void DevicesChanged_RefreshesTheDevices()
    {
        var service = CreateService();

        _transport.Attach("device-1");
        _transport.RaiseDevicesChanged();

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
    public async Task FactoryResetAsync_WriteFails_ConnectionFailed()
    {
        var (service, device) = await CreateServiceWithDeviceAsync();
        device.BeforeWrite = _ => throw new IOException();

        await service.FactoryResetAsync();

        Assert.Equal(DeviceState.ConnectionFailed, service.State);
    }

    private DeviceConnectionService CreateService(params IDac70Transport[] transports)
    {
        return new DeviceConnectionService(transports.Length > 0 ? transports : [_transport], _preferences, new FakeDispatcher());
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
}