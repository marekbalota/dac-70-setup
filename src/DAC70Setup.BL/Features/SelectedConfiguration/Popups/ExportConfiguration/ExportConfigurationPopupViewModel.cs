using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services.ImportExport;

namespace DAC70Setup.BL.Features.SelectedConfiguration.Popups.ExportConfiguration;

public partial class ExportConfigurationPopupViewModel : ObservableObject, IQueryAttributable
{
    public const string ConfigurationParameter = "Configuration";

    private readonly ConfigurationImportExportService _configurationImportExportService;
    private readonly IPopupService _popupService;

    private Configuration? _configuration;

    public ExportConfigurationPopupViewModel(ConfigurationImportExportService configurationImportExportService, IPopupService popupService)
    {
        _configurationImportExportService = configurationImportExportService;
        _popupService = popupService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYamlChosen), nameof(IsJsonChosen))]
    public partial ConfigurationExportFormat Format { get; set; }

    public bool IsYamlChosen => Format == ConfigurationExportFormat.Yaml;

    public bool IsJsonChosen => Format == ConfigurationExportFormat.Json;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChooseYamlCommand), nameof(ChooseJsonCommand), nameof(ExportCommand), nameof(CloseCommand))]
    public partial bool IsSaving { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaved))]
    public partial string? SavedFilePath { get; private set; }

    public bool IsSaved => SavedFilePath != null;

    /// <remarks>
    /// Empty when the save failed without a message; null when it has not failed.
    /// </remarks>
    [ObservableProperty]
    public partial string? ErrorMessage { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _configuration = (Configuration)query[ConfigurationParameter];
    }

    [RelayCommand(CanExecute = nameof(IsNotSaving))]
    private void ChooseYaml()
    {
        Format = ConfigurationExportFormat.Yaml;
    }

    [RelayCommand(CanExecute = nameof(IsNotSaving))]
    private void ChooseJson()
    {
        Format = ConfigurationExportFormat.Json;
    }

    [RelayCommand(CanExecute = nameof(IsNotSaving))]
    private async Task ExportAsync()
    {
        if (_configuration == null)
        {
            return;
        }

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            FileSaverResult result = await _configurationImportExportService.ExportAsync(_configuration, Format);

            if (result.IsSuccessful)
            {
                SavedFilePath = result.FilePath;
            }
            else if (!result.IsCancelled)
            {
                ErrorMessage = result.Exception.Message;
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotSaving))]
    private Task CloseAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current);
    }

    private bool IsNotSaving()
    {
        return !IsSaving;
    }
}
