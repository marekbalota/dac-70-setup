using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using DAC70Setup.BL.Features.EditConfiguration.Resources;

namespace DAC70Setup.BL.Features.EditConfiguration.Controls;

public partial class SettingSelectionView : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<Dac70SettingValue>), typeof(SettingSelectionView), null,
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(nameof(SelectedItems), typeof(ObservableCollection<Dac70SettingValue>), typeof(SettingSelectionView), null,
            propertyChanged: OnSelectedItemsChanged);

    public static readonly BindableProperty IsSelectionModeProperty =
        BindableProperty.Create(nameof(IsSelectionMode), typeof(bool), typeof(SettingSelectionView), false,
            defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnIsSelectionModeChanged);

    public static readonly BindableProperty ItemPressedCommandProperty =
        BindableProperty.Create(nameof(ItemPressedCommand), typeof(ICommand), typeof(SettingSelectionView), null);
    
    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(SettingSelectionView), null);

    private readonly ObservableCollection<SettingValueGroup> _groups = [];

    public SettingSelectionView()
    {
        PressCommand = new DelegateCommand<SettingValueItem>(OnPress);
        CancelCommand = new DelegateCommand(() => IsSelectionMode = false);

        InitializeComponent();

        Inner.ItemsSource = _groups;

        HandlerChanging += OnHandlerChanging;
    }

    public ObservableCollection<Dac70SettingValue>? ItemsSource
    {
        get => (ObservableCollection<Dac70SettingValue>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ObservableCollection<Dac70SettingValue>? SelectedItems
    {
        get => (ObservableCollection<Dac70SettingValue>?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public bool IsSelectionMode
    {
        get => (bool)GetValue(IsSelectionModeProperty);
        set => SetValue(IsSelectionModeProperty, value);
    }

    public ICommand? ItemPressedCommand
    {
        get => (ICommand?)GetValue(ItemPressedCommandProperty);
        set => SetValue(ItemPressedCommandProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public string SelectionText => SelectedCount == 0
        ? EditConfigurationPageSR.NoSettingsSelected
        : string.Format(EditConfigurationPageSR.SelectedSettingsCount, SelectedCount);

    private int SelectedCount => SelectedItems?.Count ?? 0;

    public bool IsEmpty => _groups.Count == 0;

    public DelegateCommand<SettingValueItem> PressCommand { get; }

    public DelegateCommand CancelCommand { get; }

    private IEnumerable<SettingValueItem> Rows => _groups.SelectMany(group => group);

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SettingSelectionView)bindable;

        if (oldValue is ObservableCollection<Dac70SettingValue> oldItems)
        {
            oldItems.CollectionChanged -= view.OnItemsChanged;
        }

        if (newValue is ObservableCollection<Dac70SettingValue> newItems)
        {
            newItems.CollectionChanged += view.OnItemsChanged;
        }

        view.RebuildGroups();
    }

    private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SettingSelectionView)bindable;

        if (oldValue is ObservableCollection<Dac70SettingValue> oldSelection)
        {
            oldSelection.CollectionChanged -= view.OnSelectionChanged;
        }

        if (newValue is ObservableCollection<Dac70SettingValue> newSelection)
        {
            newSelection.CollectionChanged += view.OnSelectionChanged;
        }

        view.SyncRowStates();
        view.OnPropertyChanged(nameof(SelectionText));
    }

    private static void OnIsSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SettingSelectionView)bindable;

        if (newValue is false)
        {
            view.SelectedItems?.Clear();
        }

        view.SyncRowStates();
    }

    private void OnHandlerChanging(object? sender, HandlerChangingEventArgs e)
    {
        if (e.NewHandler == null)
        {
            if (ItemsSource != null)
            {
                ItemsSource.CollectionChanged -= OnItemsChanged;
            }

            if (SelectedItems != null)
            {
                SelectedItems.CollectionChanged -= OnSelectionChanged;
            }
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGroups();
    }

    private void OnSelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncRowStates();
        OnPropertyChanged(nameof(SelectionText));
    }

    private void OnPress(SettingValueItem row)
    {
        if (IsSelectionMode)
        {
            Toggle(row.Value);

            return;
        }

        if (ItemPressedCommand?.CanExecute(row.Value) == true)
        {
            ItemPressedCommand.Execute(row.Value);
        }
    }

    private void Toggle(Dac70SettingValue value)
    {
        if (SelectedItems == null)
        {
            return;
        }

        if (SelectedItems.Contains(value))
        {
            SelectedItems.Remove(value);
        }
        else
        {
            SelectedItems.Add(value);
        }
    }

    private void RebuildGroups()
    {
        _groups.Clear();

        var settings = ItemsSource ?? [];

        List<IGrouping<PanelType, Dac70SettingValue>> groups = settings
            .GroupBy(setting => setting.Definition.Panel)
            .OrderBy(group => group.Key)
            .ToList();

        foreach (var group in groups)
        {
            _groups.Add(new SettingValueGroup(group.Key, group.Select(value => new SettingValueItem(value))));
        }

        DropMissingFromSelection(settings);

        SyncRowStates();

        OnPropertyChanged(nameof(IsEmpty));
    }

    private void DropMissingFromSelection(IEnumerable<Dac70SettingValue> settings)
    {
        if (SelectedItems == null)
        {
            return;
        }

        var present = settings.ToHashSet();

        for (int i = SelectedItems.Count - 1; i >= 0; i--)
        {
            if (!present.Contains(SelectedItems[i]))
            {
                SelectedItems.RemoveAt(i);
            }
        }
    }

    private void SyncRowStates()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = IsSelectionMode && SelectedItems?.Contains(row.Value) == true;
        }
    }
}
