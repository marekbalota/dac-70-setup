using System.Collections.Specialized;
using System.ComponentModel;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.SettingSelection;

public class SettingSelectionViewModel : BindableBase, IDisposable
{
    private readonly DataModel _model;
    private readonly IConfigurationsService _configurationsService;
    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly Func<Dac70SettingValue, Task> _openSetting;

    private readonly List<SettingRowItem> _rows;

    private Configuration? _observedConfiguration;
    private DeviceSnapshot? _observedSnapshot;

    public SettingSelectionViewModel(DataModel model, IConfigurationsService configurationsService,
        DeviceConnectionService deviceConnectionService, ILocalizationResourceManager localizationResourceManager,
        Func<Dac70SettingValue, Task> openSetting)
    {
        _model = model;
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

        PressCommand = new AsyncDelegateCommand<SettingRowItem>(async row =>
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
        });

        LongPressCommand = new DelegateCommand<SettingRowItem>(row =>
        {
            if (row.IsIncluded)
            {
                SetSelected([row], true);
            }
        });

        ToggleIncludedCommand = new AsyncDelegateCommand<SettingRowItem>(async row =>
        {
            if (IsSelectionMode || _model.SelectedConfiguration is not { } configuration)
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
        });

        SelectAllCommand = new DelegateCommand(() => SetSelected(_rows.Where(row => row.IsIncluded), true));

        ClearSelectionCommand = new DelegateCommand(() => SetSelected(_rows, false));

        UseDeviceValuesCommand = new AsyncDelegateCommand(async () =>
        {
            if (_model.SelectedConfiguration is not { } configuration)
            {
                return;
            }

            var values = SelectedRows
                .Select(row => row.DeviceValue)
                .OfType<Dac70SettingValue>()
                .ToList();

            SetSelected(_rows, false);

            await _configurationsService.SaveSettingsAsync(configuration, values);
        }, () => IsSelectionMode && SelectedRows.Any(row => row.DeviceValue != null)
            && _deviceConnectionService.State is not (DeviceState.Sending or DeviceState.Syncing));

        SendSelectedCommand = new AsyncDelegateCommand(async () =>
        {
            var values = SelectedRows
                .Select(row => row.ConfigurationValue)
                .OfType<Dac70SettingValue>()
                .ToList();

            SetSelected(_rows, false);

            await _deviceConnectionService.SendValuesAsync(values);
        }, () => IsSelectionMode && _deviceConnectionService.CanSend);

        _model.PropertyChanged += OnModelPropertyChanged;
        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        ObserveSelectedConfiguration();
        ObserveSelectedSnapshot();
    }

    public AsyncDelegateCommand<SettingRowItem> PressCommand { get; }

    public DelegateCommand<SettingRowItem> LongPressCommand { get; }

    public AsyncDelegateCommand<SettingRowItem> ToggleIncludedCommand { get; }

    public DelegateCommand SelectAllCommand { get; }

    public DelegateCommand ClearSelectionCommand { get; }

    public AsyncDelegateCommand UseDeviceValuesCommand { get; }

    public AsyncDelegateCommand SendSelectedCommand { get; }

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

        RaisePropertyChanged(nameof(SelectedCount));
        RaisePropertyChanged(nameof(IsSelectionMode));
        UseDeviceValuesCommand.RaiseCanExecuteChanged();
        SendSelectedCommand.RaiseCanExecuteChanged();
    }

    private void ObserveSelectedConfiguration()
    {
        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
        }

        _observedConfiguration = _model.SelectedConfiguration;

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

        UseDeviceValuesCommand.RaiseCanExecuteChanged();
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataModel.SelectedConfiguration))
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

        UseDeviceValuesCommand.RaiseCanExecuteChanged();
        SendSelectedCommand.RaiseCanExecuteChanged();
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

    public void Dispose()
    {
        _model.PropertyChanged -= OnModelPropertyChanged;
        _deviceConnectionService.PropertyChanged -= OnDeviceConnectionServiceChanged;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
            _observedConfiguration = null;
        }

        if (_observedSnapshot != null)
        {
            _observedSnapshot.PropertyChanged -= OnSnapshotPropertyChanged;
            _observedSnapshot = null;
        }

        foreach (var row in _rows)
        {
            row.Dispose();
        }
    }
}
