namespace DAC70Setup.BL.Services.Commands;

public class ApplicationCommands : IApplicationCommands
{
    public ApplicationCommands()
    {
        AddCommand = new CompositeCommand(true);
        RemoveCommand = new CompositeCommand(true);
        SelectCommand = new CompositeCommand(true);
    }

    public CompositeCommand AddCommand { get; }

    public CompositeCommand RemoveCommand { get; }

    public CompositeCommand SelectCommand { get; }
}
