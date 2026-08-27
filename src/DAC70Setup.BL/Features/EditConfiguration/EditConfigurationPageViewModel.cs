using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DAC70Setup.BL.Features.Configurations;
using DAC70Setup.BL.Features.EditConfiguration.Commands;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Commands;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Features.EditConfiguration;

public class EditConfigurationPageViewModel : TabViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IApplicationCommands _applicationCommands;

    private Configuration? _observedConfiguration;

    public EditConfigurationPageViewModel(DataModel model,
        INavigationService navigationService,
        IApplicationCommands applicationCommands,
        AddSettingCommand addSettingCommand,
        DeleteSettingCommand deleteSettingCommand,
        SelectSettingCommand selectSettingCommand)
    {
        Model = model;
        _navigationService = navigationService;
        _applicationCommands = applicationCommands;

        GoToConfigurationsCommand = new DelegateCommand(async () =>
            await _navigationService.SelectTabAsync(nameof(ConfigurationsPage)));

        AddSettingCommand = addSettingCommand;
        DeleteSettingCommand = deleteSettingCommand;
        SelectSettingCommand = selectSettingCommand;

        _applicationCommands.AddCommand.RegisterCommand(AddSettingCommand);
        _applicationCommands.RemoveCommand.RegisterCommand(DeleteSettingCommand);
        _applicationCommands.SelectCommand.RegisterCommand(SelectSettingCommand);

        Model.PropertyChanged += OnModelPropertyChanged;

        ObserveSelectedConfiguration();
    }

    public DataModel Model { get; }

    public DelegateCommand GoToConfigurationsCommand { get; }

    public CommandBase AddSettingCommand { get; }

    public CommandBase DeleteSettingCommand { get; }

    public CommandBase SelectSettingCommand { get; }

    public ObservableCollection<SettingValueGroup> SettingGroups { get; } = [];

    public ObservableCollection<object> SelectedSettings { get; } = [];

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataModel.SelectedConfiguration))
        {
            ObserveSelectedConfiguration();
        }
    }

    private void ObserveSelectedConfiguration()
    {
        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
        }

        _observedConfiguration = Model.SelectedConfiguration;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged += OnSettingsCollectionChanged;
        }

        SelectedSettings.Clear();

        SyncSettingGroups();
    }

    private void OnSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncSettingGroups();
    }

    private void SyncSettingGroups()
    {
        var settings = Model.SelectedConfiguration?.Settings ?? [];

        List<IGrouping<PanelType, Dac70SettingValue>> groups = settings
            .GroupBy(s => s.Definition.Panel)
            .OrderBy(g => g.Key)
            .ToList();

        for (int i = SettingGroups.Count - 1; i >= 0; i--)
        {
            if (groups.All(g => g.Key != SettingGroups[i].Panel))
            {
                SettingGroups.RemoveAt(i);
            }
        }

        for (int i = 0; i < groups.Count; i++)
        {
            var group = SettingGroups.FirstOrDefault(g => g.Panel == groups[i].Key);

            if (group == null)
            {
                SettingGroups.Insert(i, new SettingValueGroup(groups[i].Key, groups[i]));
            }
            else
            {
                SyncGroupValues(group, groups[i]);
            }
        }
    }

    private static void SyncGroupValues(SettingValueGroup group, IEnumerable<Dac70SettingValue> values)
    {
        var target = values.ToList();

        for (int i = group.Count - 1; i >= 0; i--)
        {
            if (!target.Contains(group[i]))
            {
                group.RemoveAt(i);
            }
        }

        for (int i = 0; i < target.Count; i++)
        {
            if (!group.Contains(target[i]))
            {
                group.Insert(i, target[i]);
            }
        }
    }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        AddSettingCommand.Initialize(this);
        DeleteSettingCommand.Initialize(this);
        SelectSettingCommand.Initialize(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Model.PropertyChanged -= OnModelPropertyChanged;

        if (_observedConfiguration != null)
        {
            _observedConfiguration.Settings.CollectionChanged -= OnSettingsCollectionChanged;
            _observedConfiguration = null;
        }

        _applicationCommands.AddCommand.UnregisterCommand(AddSettingCommand);
        _applicationCommands.RemoveCommand.UnregisterCommand(DeleteSettingCommand);
        _applicationCommands.SelectCommand.UnregisterCommand(SelectSettingCommand);

        AddSettingCommand.Destroy();
        DeleteSettingCommand.Destroy();
        SelectSettingCommand.Destroy();
    }
}
