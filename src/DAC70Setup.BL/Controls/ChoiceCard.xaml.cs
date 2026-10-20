using System.Windows.Input;

namespace DAC70Setup.BL.Controls;

public partial class ChoiceCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ChoiceCard), string.Empty);

    public static readonly BindableProperty DetailProperty =
        BindableProperty.Create(nameof(Detail), typeof(string), typeof(ChoiceCard), string.Empty);

    public static readonly BindableProperty IsChosenProperty =
        BindableProperty.Create(nameof(IsChosen), typeof(bool), typeof(ChoiceCard), false);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChoiceCard), propertyChanged: OnCommandChanged);

    public ChoiceCard()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public bool IsChosen
    {
        get => (bool)GetValue(IsChosenProperty);
        set => SetValue(IsChosenProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && (Command?.CanExecute(null) ?? true);

    private static void OnCommandChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var card = (ChoiceCard)bindable;

        if (oldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= card.OnCanExecuteChanged;
        }

        if (newValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += card.OnCanExecuteChanged;
        }

        card.RefreshIsEnabledProperty();
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e)
    {
        RefreshIsEnabledProperty();
    }
}
