using DAC70Setup.BL.Features.Configurations.Commands;
using DAC70Setup.BL.Features.SelectedConfiguration;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Models.Commands;
using DAC70Setup.BL.Models.ViewModels;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Commands;

namespace DAC70Setup.BL.Features.Configurations;

public class ConfigurationsPageViewModel : TabViewModelBase
{
    private readonly IApplicationCommands _applicationCommands;
    private readonly IConfigurationsService _configurationsService;
    private readonly INavigationService _navigationService;

    public ConfigurationsPageViewModel(DataModel model,
        IApplicationCommands applicationCommands,
        CreateConfigurationCommand createConfigurationCommand,
        DeleteConfigurationCommand deleteConfigurationCommand,
        SelectConfigurationCommand selectConfigurationCommand,
        IConfigurationsService configurationsService,
        INavigationService navigationService)
    {
        Model = model;
        _applicationCommands = applicationCommands;
        _configurationsService = configurationsService;
        _navigationService = navigationService;

        CreateConfigurationCommand = createConfigurationCommand;
        DeleteConfigurationCommand = deleteConfigurationCommand;
        SelectConfigurationCommand = selectConfigurationCommand;

        _applicationCommands.SelectCommand.RegisterCommand(SelectConfigurationCommand);
        _applicationCommands.RemoveCommand.RegisterCommand(DeleteConfigurationCommand);
        _applicationCommands.AddCommand.RegisterCommand(CreateConfigurationCommand);

        OpenConfigurationCommand = new DelegateCommand<Configuration>(async configuration =>
        {
            _configurationsService.SelectConfiguration(configuration);
            await _navigationService.SelectTabAsync(nameof(SelectedConfigurationPage));
        });
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

    public DelegateCommand<Configuration> OpenConfigurationCommand { get; }

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
