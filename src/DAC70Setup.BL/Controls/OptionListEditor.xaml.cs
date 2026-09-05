using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.BL.Controls;

public partial class OptionListEditor : ContentView
{
    public static readonly BindableProperty OptionsProperty = BindableProperty.Create(
        nameof(Options), typeof(IReadOnlyList<EnumOption>), typeof(OptionListEditor), null);

    public static readonly BindableProperty SelectedOptionProperty = BindableProperty.Create(
        nameof(SelectedOption), typeof(EnumOption), typeof(OptionListEditor), null, BindingMode.TwoWay);

    public OptionListEditor()
    {
        InitializeComponent();
    }

    public IReadOnlyList<EnumOption>? Options
    {
        get => (IReadOnlyList<EnumOption>?)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public EnumOption? SelectedOption
    {
        get => (EnumOption?)GetValue(SelectedOptionProperty);
        set => SetValue(SelectedOptionProperty, value);
    }
}
