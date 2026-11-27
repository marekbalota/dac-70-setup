using DAC70Setup.BL.Mappers;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;
using DAC70Setup.Data;
using DAC70Setup.Tests.Fakes;
using SQLite;

namespace DAC70Setup.Tests.BL.Services;

public class ConfigurationsServiceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"dac70setup-tests-{Guid.NewGuid()}.db3");
    private readonly FakePreferences _preferences = new();
    private readonly EnumSettingDefinition _inputSource = Catalog.Get<EnumSettingDefinition>("Input Source");
    private readonly RangeSettingDefinition _brightness = Catalog.Get<RangeSettingDefinition>("Brightness");

    [Fact]
    public async Task CreateNewConfigurationAsync_IsThereAfterRestart()
    {
        var service = await StartServiceAsync();

        await service.CreateNewConfigurationAsync("Studio");

        var restarted = await StartServiceAsync();
        Assert.Equal("Studio", Assert.Single(restarted.Configurations).Name);
    }

    // Changing the snapshot afterwards would also change the configuration if it shared the device's values
    [Fact]
    public async Task CreateFromDeviceAsync_CopiesTheDeviceValues()
    {
        var service = await StartServiceAsync();
        var snapshot = new DeviceSnapshot();
        snapshot.SetValue(CreateInputSource("SDI"));
        snapshot.SetValue(CreateBrightness(0x90));

        await service.CreateFromDeviceAsync("From device", snapshot);
        snapshot.SetValue(CreateBrightness(0x10));

        var restarted = await StartServiceAsync();
        Dictionary<string, string> expected = new() { ["Input Source"] = "SDI", ["Brightness"] = "144" };
        Assert.Equal(expected, Describe(Assert.Single(service.Configurations)));
        Assert.Equal(expected, Describe(Assert.Single(restarted.Configurations)));
    }

    [Fact]
    public async Task CreateCopyAsync_IsIndependentOfTheOriginal()
    {
        var service = await StartServiceAsync();
        await service.CreateFromFileAsync("Original", [CreateBrightness(0x90)]);
        var original = Assert.Single(service.Configurations);

        await service.CreateCopyAsync("Copy", original);
        await service.SaveSettingAsync(original, CreateBrightness(0x10));

        var restarted = await StartServiceAsync();
        Assert.Equal("144", Assert.Single(service.Configurations.Single(configuration => configuration.Name == "Copy").Settings).DisplayValueText);
        Assert.Equal("144", Assert.Single(restarted.Configurations.Single(configuration => configuration.Name == "Copy").Settings).DisplayValueText);
    }

    [Fact]
    public async Task SaveSettingAsync_SameSettingTwice_KeepsOnlyTheLastValue()
    {
        var service = await StartServiceAsync();
        await service.CreateNewConfigurationAsync("Studio");
        var configuration = Assert.Single(service.Configurations);

        await service.SaveSettingAsync(configuration, CreateBrightness(0x90));
        await service.SaveSettingAsync(configuration, CreateBrightness(0x10));

        var restarted = await StartServiceAsync();
        Assert.Equal("16", Assert.Single(configuration.Settings).DisplayValueText);
        Assert.Equal("16", Assert.Single(Assert.Single(restarted.Configurations).Settings).DisplayValueText);
    }

    [Fact]
    public async Task DeleteSettingAsync_IsGoneAfterRestart()
    {
        var service = await StartServiceAsync();
        await service.CreateFromFileAsync("Studio", [CreateInputSource("SDI"), CreateBrightness(0x90)]);
        var configuration = Assert.Single(service.Configurations);

        await service.DeleteSettingAsync(configuration, configuration.Settings.Single(value => value.Definition == _brightness));

        var restarted = await StartServiceAsync();
        Assert.Equal(_inputSource, Assert.Single(configuration.Settings).Definition);
        Assert.Equal(_inputSource, Assert.Single(Assert.Single(restarted.Configurations).Settings).Definition);
    }

    [Fact]
    public async Task RenameConfigurationAsync_IsKeptAfterRestart()
    {
        var service = await StartServiceAsync();
        await service.CreateNewConfigurationAsync("Studio");

        await service.RenameConfigurationAsync(service.Configurations[0], "Live");

        var restarted = await StartServiceAsync();
        Assert.Equal("Live", service.Configurations[0].Name);
        Assert.Equal("Live", Assert.Single(restarted.Configurations).Name);
    }

    [Fact]
    public async Task SelectConfiguration_IsRememberedForTheNextStart()
    {
        var service = await StartServiceAsync();
        await service.CreateNewConfigurationAsync("Studio");
        await service.CreateNewConfigurationAsync("Live");

        service.SelectConfiguration(service.Configurations.Single(configuration => configuration.Name == "Live"));

        var restarted = await StartServiceAsync();
        Assert.Equal("Live", restarted.SelectedConfiguration?.Name);
        Assert.True(restarted.SelectedConfiguration?.IsActive);
    }

    [Fact]
    public async Task SelectConfiguration_Other_MovesTheActiveMark()
    {
        var service = await StartServiceAsync();
        await service.CreateNewConfigurationAsync("Studio");
        await service.CreateNewConfigurationAsync("Live");
        service.SelectConfiguration(service.Configurations[0]);
        Assert.True(service.Configurations[0].IsActive);

        service.SelectConfiguration(service.Configurations[1]);

        Assert.False(service.Configurations[0].IsActive);
        Assert.True(service.Configurations[1].IsActive);
    }

    [Fact]
    public async Task DeleteConfigurationAsync_Selected_ClearsTheSelectionAndIsGoneAfterRestart()
    {
        var service = await StartServiceAsync();
        await service.CreateNewConfigurationAsync("Studio");
        await service.CreateNewConfigurationAsync("Live");
        var studio = service.Configurations.Single(configuration => configuration.Name == "Studio");
        service.SelectConfiguration(studio);

        await service.DeleteConfigurationAsync(studio);

        var restarted = await StartServiceAsync();
        Assert.Null(service.SelectedConfiguration);
        Assert.Equal("Live", Assert.Single(restarted.Configurations).Name);
    }

    // A storage can hold a setting this version of the app does not know, or bytes it cannot read
    [Theory]
    [InlineData(0x7F, new byte[] { 0x01 })]
    [InlineData(0x04, new byte[] { 0x01, 0x02 })]
    public async Task InitializeAsync_UnreadableStoredSetting_IsSkipped(int settingId, byte[] valueBytes)
    {
        var service = await StartServiceAsync();
        await service.CreateFromFileAsync("Studio", [CreateBrightness(0x90)]);
        var repository = new ConfigurationRepository(_databasePath);
        await repository.UpsertSettingsAsync(service.Configurations[0].Id,
            [new SettingDto(new SettingKey(MessageType.C0, (byte)settingId), valueBytes)]);

        var restarted = await StartServiceAsync();

        Assert.Equal(_brightness, Assert.Single(Assert.Single(restarted.Configurations).Settings).Definition);
    }

    private async Task<ConfigurationsService> StartServiceAsync()
    {
        var settingMapper = new SettingMapper();
        var service = new ConfigurationsService(new ConfigurationRepository(_databasePath),
            new ConfigurationMapper(settingMapper), settingMapper, _preferences);
        await service.InitializeAsync();

        return service;
    }

    private EnumSettingValue CreateInputSource(string optionName)
    {
        return new EnumSettingValue(_inputSource) { SelectedOption = Catalog.GetOption(_inputSource, optionName) };
    }

    private RangeSettingValue CreateBrightness(byte value)
    {
        return new RangeSettingValue(_brightness) { Value = value };
    }

    private static Dictionary<string, string> Describe(Configuration configuration)
    {
        return configuration.Settings.ToDictionary(value => value.Definition.Name, value => value.DisplayValueText);
    }

    public void Dispose()
    {
        SQLiteAsyncConnection.ResetPool();
        File.Delete(_databasePath);
    }
}