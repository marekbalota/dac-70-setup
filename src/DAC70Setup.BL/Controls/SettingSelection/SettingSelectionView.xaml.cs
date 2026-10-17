namespace DAC70Setup.BL.Controls.SettingSelection;

public partial class SettingSelectionView : ContentView
{
    public static readonly BindableProperty SelectionProperty = BindableProperty.Create(
        nameof(Selection), typeof(SettingSelectionViewModel), typeof(SettingSelectionView));

    public SettingSelectionViewModel? Selection
    {
        get => (SettingSelectionViewModel?)GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
    }

    public SettingSelectionView()
    {
        InitializeComponent();
    }
}
