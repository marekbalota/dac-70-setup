using System.ComponentModel;
using DAC70Setup.BL.Resources;

namespace DAC70Setup.BL.Controls.DeviceStatus;

public partial class DeviceStatusBar : ContentView
{
    private const double OptionHeight = 57;
    private const double MaxVisibleOptions = 4.5;

    private CollectionView? _optionList;
    private View? _optionOverlay;
    private Label? _listArrow;

    private bool _isListOpen;

    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(DeviceStatusBarViewModel), typeof(DeviceStatusBar), null, propertyChanged: OnStatusChanged);

    public DeviceStatusBarViewModel? Status
    {
        get => (DeviceStatusBarViewModel?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public DeviceStatusBar()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _optionList = GetTemplateChild("OptionList") as CollectionView;
        _optionOverlay = GetTemplateChild("OptionOverlay") as View;
        _listArrow = GetTemplateChild("ListArrow") as Label;

        UpdateOptionList();
        SetListOpen(false);
    }

    private static void OnStatusChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var deviceBar = (DeviceStatusBar)bindable;

        if (oldValue is DeviceStatusBarViewModel oldStatus)
        {
            oldStatus.PropertyChanged -= deviceBar.OnStatusPropertyChanged;
        }

        if (newValue is DeviceStatusBarViewModel newStatus)
        {
            newStatus.PropertyChanged += deviceBar.OnStatusPropertyChanged;
        }

        deviceBar.UpdateOptionList();
        deviceBar.SetListOpen(false);
    }

    private void OnStatusPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceStatusBarViewModel.Devices))
        {
            UpdateOptionList();
        }
        else if (e.PropertyName is nameof(DeviceStatusBarViewModel.IsPickerAvailable) or nameof(DeviceStatusBarViewModel.IsPickerEnabled)
            && !CanOpenList())
        {
            SetListOpen(false);
        }
    }

    private void OnListToggleTapped(object? sender, TappedEventArgs e)
    {
        SetListOpen(!_isListOpen && CanOpenList());
    }

    private void OnScrimTapped(object? sender, TappedEventArgs e)
    {
        SetListOpen(false);
    }

    private void OnOptionTapped(object? sender, TappedEventArgs e)
    {
        SetListOpen(false);
    }

    private bool CanOpenList()
    {
        return Status is { IsPickerAvailable: true, IsPickerEnabled: true };
    }

    private void SetListOpen(bool isOpen)
    {
        _isListOpen = isOpen;

        if (_optionOverlay != null)
        {
            _optionOverlay.IsVisible = isOpen;
        }

        if (_listArrow != null)
        {
            _listArrow.Text = isOpen ? AwesomeSolidIconFont.ChevronUp : AwesomeSolidIconFont.ChevronDown;
        }

        if (isOpen)
        {
            ScrollToSelected();
        }
    }

    private void UpdateOptionList()
    {
        if (_optionList is { } optionList && Status is { } status)
        {
            optionList.HeightRequest = Math.Min(status.Devices.Count, MaxVisibleOptions) * OptionHeight;
        }
    }

    private void ScrollToSelected()
    {
        if (_optionList is { } optionList && Status is { } status
            && status.Devices.Count > MaxVisibleOptions && status.Devices.FirstOrDefault(device => device.IsSelected) is { } selected)
        {
            Dispatcher.Dispatch(() => optionList.ScrollTo(selected, position: ScrollToPosition.MakeVisible, animate: false));
        }
    }
}
