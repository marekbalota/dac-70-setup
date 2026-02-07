namespace DAC70Setup.BL.Services.Commands;

public interface IApplicationCommands
{
    CompositeCommand AddCommand { get; }
    CompositeCommand RemoveCommand { get; }
    CompositeCommand SelectCommand { get; }
}
