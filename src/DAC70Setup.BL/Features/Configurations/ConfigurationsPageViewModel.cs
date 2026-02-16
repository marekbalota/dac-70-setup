using DAC70Setup.BL.Features.Configurations.Commands;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services.Commands;

namespace DAC70Setup.BL.Features.Configurations;

public class ConfigurationsPageViewModel : TabViewModelBase
{
    private readonly IApplicationCommands _applicationCommands;

    public ConfigurationsPageViewModel(DataModel model,
        IApplicationCommands applicationCommands,
        CreateConfigurationCommand createConfigurationCommand,
        DeleteConfigurationCommand deleteConfigurationCommand,
        SelectConfigurationCommand selectConfigurationCommand)
    {
        Model = model;
        _applicationCommands = applicationCommands;

        CreateConfigurationCommand = createConfigurationCommand;
        DeleteConfigurationCommand = deleteConfigurationCommand;
        SelectConfigurationCommand = selectConfigurationCommand;

        _applicationCommands.SelectCommand.RegisterCommand(SelectConfigurationCommand);
        _applicationCommands.RemoveCommand.RegisterCommand(DeleteConfigurationCommand);
        _applicationCommands.AddCommand.RegisterCommand(CreateConfigurationCommand);
    }

    public DataModel Model { get; }

    private Configuration? _selectedConfiguration;
    public Configuration? SelectedConfiguration
    {
        get => _selectedConfiguration;
        set => SetProperty(ref _selectedConfiguration, value);
    }

    public CommandBase CreateConfigurationCommand { get; }

    public CommandBase DeleteConfigurationCommand { get; }

    public CommandBase SelectConfigurationCommand { get; }

    protected override async Task InitializeAsyncInt(INavigationParameters parameters)
    {
        await base.InitializeAsyncInt(parameters);

        SelectConfigurationCommand.Initialize(this);
        DeleteConfigurationCommand.Initialize(this);
        CreateConfigurationCommand.Initialize(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _applicationCommands.SelectCommand.UnregisterCommand(SelectConfigurationCommand);
        _applicationCommands.RemoveCommand.UnregisterCommand(DeleteConfigurationCommand);
        _applicationCommands.AddCommand.UnregisterCommand(CreateConfigurationCommand);
    }
}
