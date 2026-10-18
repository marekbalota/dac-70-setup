using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.SettingSelection;

public partial class SettingSelectionViewModel : ObservableObject
{
    private readonly ConfigurationsService _configurationsService;
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly Func<Dac70SettingValue, Task> _openSetting;

    private readonly List<SettingRowItem> _rows;

    private Configuration? _observedConfiguration;
    private DeviceSnapshot? _observedSnapshot;

    public SettingSelectionViewModel(ConfigurationsService configurationsService,
        DeviceConnectionService deviceConnectionService, ILocalizationResourceManager localizationResourceManager,
        Func<Dac70SettingValue, Task> openSetting)
    {
        _configurationsService = configurationsService;
        _deviceConnectionService = deviceConnectionService;
        _openSetting = openSetting;

        _rows = Dac70SettingCatalog.All
            .Select(definition => new SettingRowItem(definition, localizationResourceManager))
            .ToList();

        Groups = _rows
            .GroupBy(row => row.Definition.Panel)
            .OrderBy(group => group.Key)
            .Select(group => new SettingRowGroup(group.Key, group))
            .ToList();

        _configurationsService.PropertyChanged += OnConfigurationsPropertyChanged;
        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        ObserveSelectedConfiguration();
        ObserveSelectedSnapshot();
    }

    [RelayCommand]
    private async Task PressAsync(SettingRowItem row)
    {
        if (IsSelectionMode)
        {
            if (row.IsIncluded)
            {
                SetSelected([row], !row.IsSelected);
            }
        }
        else if (row.ConfigurationValue is { } value)
        {
            await _openSetting(value);
        }
    }

    [RelayCommand]
    private void LongPress(SettingRowItem row)
    {
        if (row.IsIncluded)
        {
            SetSelected([row], true);
        }
    }

    [RelayCommand]
    private async Task ToggleIncludedAsync(SettingRowItem row)
    {
        if (IsSelectionMode || _configurationsService.SelectedConfiguration is not { } configuration)
        {
            return;
        }

        if (row.ConfigurationValue is { } value)
        {
            await _configurationsService.DeleteSettingAsync(configuration, value);
        }
        else if (row.DeviceValue is { } deviceValue)
        {
            await _configurationsService.SaveSettingAsync(configuration, deviceValue);
        }
        else
        {
            await _openSetting(row.Definition.CreateValue());
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        SetSelected(_rows.Where(row => row.IsIncluded), true);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SetSelected(_rows, false);
    }

    [RelayCommand(CanExecute = nameof(CanUseDeviceValues))]
    private async Task UseDeviceValuesAsync()
    {
        if (_configurationsService.SelectedConfiguration is not { } configuration)
        {
            return;
        }

        var values = SelectedRows
            .Select(row => row.DeviceValue)
            .OfType<Dac70SettingValue>()
            .ToList();

        SetSelected(_rows, false);

        await _configurationsService.SaveSettingsAsync(configuration, values);
    }

    private bool CanUseDeviceValues()
    {
        return IsSelectionMode && SelectedRows.Any(row => row.DeviceValue != null)
            && _deviceConnectionService.State is not (DeviceState.Sending or DeviceState.Syncing);
    }

    [RelayCommand(CanExecute = nameof(CanSendSelected))]
    private async Task SendSelectedAsync()
    {
        var values = SelectedRows
            .Select(row => row.ConfigurationValue)
            .OfType<Dac70SettingValue>()
            .ToList();

        SetSelected(_rows, false);

        await _deviceConnectionService.SendValuesAsync(values);
    }

    private bool CanSendSelected()
    {
        return IsSelectionMode && _deviceConnectionService.CanSend;
    }

    public IReadOnlyList<SettingRowGroup> Groups { get; }

    public int SelectedCount => SelectedRows.Count();

    public bool IsSelectionMode => SelectedRows.Any();

    private IEnumerable<SettingRowItem> SelectedRows => _rows.Where(row => row.IsSelected);

    private void SetSelected(IEnumerable<SettingRowItem> rows, bool isSelected)
    {
        var changedRows = rows.Where(row => row.IsSelected != isSelected).ToList();
        if (changedRows.Count == 0)
        {
            return;
        }

        foreach (var row in changedRows)
        {
            row.IsSelected = isSelected;
        }

        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(IsSelectionMode));
        UseDeviceValuesCommand.NotifyCanExecuteChanged();
        SendSelectedCommand.NotifyCanExecuteChanged();
    }

    private void ObserveSelectedConfiguration()
    {
        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
        }

        _observedConfiguration = _configurationsService.SelectedConfiguration;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged += OnSettingsCollectionChanged;
        }

        RefreshRows();
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

        RefreshRows();
    }

    private void RefreshRows()
    {
        foreach (var row in _rows)
        {
            var key = row.Definition.Key;

            row.ConfigurationValue = _observedConfiguration?.Settings.FirstOrDefault(setting => setting.Definition.Key == key);
            row.DeviceValue = _observedSnapshot?.Values.GetValueOrDefault(key);
        }

        SetSelected(_rows.Where(row => !row.IsIncluded), false);

        UseDeviceValuesCommand.NotifyCanExecuteChanged();
    }

    private void OnConfigurationsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfigurationsService.SelectedConfiguration))
        {
            SetSelected(_rows, false);
            ObserveSelectedConfiguration();
        }
    }

    private void OnDeviceConnectionServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceConnectionService.SelectedDevice))
        {
            ObserveSelectedSnapshot();
        }

        UseDeviceValuesCommand.NotifyCanExecuteChanged();
        SendSelectedCommand.NotifyCanExecuteChanged();
    }

    private void OnSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshRows();
    }

    private void OnSnapshotPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceSnapshot.Values))
        {
            RefreshRows();
        }
    }
}
