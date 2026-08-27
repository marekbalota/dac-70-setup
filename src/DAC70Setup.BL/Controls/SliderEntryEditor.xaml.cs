namespace DAC70Setup.BL.Controls;

public partial class SliderEntryEditor : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(int), typeof(SliderEntryEditor), 0, BindingMode.TwoWay,
        propertyChanged: OnValueChanged, coerceValue: CoerceValue);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(int), typeof(SliderEntryEditor), 0, propertyChanged: OnRangeChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(int), typeof(SliderEntryEditor), 100, propertyChanged: OnRangeChanged);

    public SliderEntryEditor()
    {
        InitializeComponent();

        UpdateEntryText();
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static object CoerceValue(BindableObject bindable, object value)
    {
        var editor = (SliderEntryEditor)bindable;

        return Math.Min(editor.Maximum, Math.Max(editor.Minimum, (int)value));
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((SliderEntryEditor)bindable).UpdateEntryText();
    }

    private static void OnRangeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        bindable.CoerceValue(ValueProperty);
    }

    private void OnValueEntryCompleted(object? sender, EventArgs e)
    {
        CommitEntryText();
    }

    private void OnValueEntryUnfocused(object? sender, FocusEventArgs e)
    {
        CommitEntryText();
    }

    private void CommitEntryText()
    {
        if (int.TryParse(ValueEntry.Text, out int typed))
        {
            Value = typed;
        }

        UpdateEntryText();
    }

    private void UpdateEntryText()
    {
        ValueEntry.Text = Value.ToString();
    }
}
