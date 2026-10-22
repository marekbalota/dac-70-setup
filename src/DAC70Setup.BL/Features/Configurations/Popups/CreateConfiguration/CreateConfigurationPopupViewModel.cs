using System.ComponentModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration.Resources;
using DAC70Setup.BL.Models;
using DAC70Setup.BL.Services;
using DAC70Setup.BL.Services.Device;
using DAC70Setup.BL.Services.ImportExport;
using DAC70Setup.Core.Models.Values;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Popups.CreateConfiguration;

public partial class CreateConfigurationPopupViewModel : ObservableObject, IQueryAttributable
{
    public const string CopiedConfigurationParameter = "CopiedConfiguration";

    private const int ShownImportNoteCount = 2;

    private readonly DeviceConnectionService _deviceConnectionService;
    private readonly ConfigurationImportExportService _configurationImportExportService;
    private readonly IPopupService _popupService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    private string? _nameFromFile;

    public CreateConfigurationPopupViewModel(ConfigurationsService configurationsService,
        DeviceConnectionService deviceConnectionService,
        ConfigurationImportExportService configurationImportExportService,
        IPopupService popupService,
        ILocalizationResourceManager localizationResourceManager)
    {
        ConfigurationsService = configurationsService;
        _deviceConnectionService = deviceConnectionService;
        _configurationImportExportService = configurationImportExportService;
        _popupService = popupService;
        _localizationResourceManager = localizationResourceManager;

        Name = string.Empty;
        AllImportNotes = [];
        DeviceDetail = new LocalizedString(() => string.Empty);
        CopiedConfiguration = configurationsService.SelectedConfiguration ?? configurationsService.Configurations.FirstOrDefault();

        string copyDetailKey = configurationsService.Configurations.Count > 0
            ? nameof(CreateConfigurationPopupSR.NewConfigurationCopyDetail)
            : nameof(CreateConfigurationPopupSR.NewConfigurationCopyUnavailable);
        CopyDetail = new LocalizedString(() => _localizationResourceManager[copyDetailKey]);

        _deviceConnectionService.PropertyChanged += OnDeviceConnectionServiceChanged;

        UpdateDevice();

        if (IsDeviceAvailable)
        {
            Source = ConfigurationSource.Device;
        }
    }

    public ConfigurationsService ConfigurationsService { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeviceChosen), nameof(IsCopyChosen), nameof(IsEmptyChosen), nameof(IsFileChosen))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial ConfigurationSource Source { get; set; }

    public bool IsDeviceChosen => Source == ConfigurationSource.Device;

    public bool IsCopyChosen => Source == ConfigurationSource.Copy;

    public bool IsEmptyChosen => Source == ConfigurationSource.Empty;

    public bool IsFileChosen => Source == ConfigurationSource.File;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial Configuration? CopiedConfiguration { get; set; }

    public LocalizedString CopyDetail { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChooseDeviceCommand))]
    public partial bool IsDeviceAvailable { get; private set; }

    [ObservableProperty]
    public partial LocalizedString DeviceDetail { get; private set; }

    [ObservableProperty]
    public partial bool IsReadingFile { get; private set; }

    [ObservableProperty]
    public partial string? ImportFileName { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial IReadOnlyList<Dac70SettingValue>? ImportedValues { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImportNotes), nameof(ImportNotesToggle))]
    public partial IReadOnlyList<LocalizedString> AllImportNotes { get; private set; }

    partial void OnAllImportNotesChanged(IReadOnlyList<LocalizedString> value)
    {
        AreAllImportNotesShown = false;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImportNotes), nameof(ImportNotesToggle))]
    public partial bool AreAllImportNotesShown { get; private set; }

    public IReadOnlyList<LocalizedString> ImportNotes => AreAllImportNotesShown
        ? AllImportNotes
        : AllImportNotes.Take(ShownImportNoteCount).ToList();

    public LocalizedString? ImportNotesToggle
    {
        get
        {
            int hiddenCount = AllImportNotes.Count - ShownImportNoteCount;

            if (hiddenCount <= 0)
            {
                return null;
            }

            return AreAllImportNotesShown
                ? new LocalizedString(() => _localizationResourceManager[nameof(CreateConfigurationPopupSR.NewConfigurationFileFewerNotes)])
                : new LocalizedString(() => _localizationResourceManager[nameof(CreateConfigurationPopupSR.NewConfigurationFileMoreNotes), hiddenCount]);
        }
    }

    [ObservableProperty]
    public partial LocalizedString? ImportError { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(CopiedConfigurationParameter, out object? value) && value is Configuration configuration)
        {
            CopiedConfiguration = configuration;
        }
    }

    [RelayCommand(CanExecute = nameof(IsDeviceAvailable))]
    private void ChooseDevice()
    {
        Source = ConfigurationSource.Device;
    }

    [RelayCommand(CanExecute = nameof(CanChooseCopy))]
    private void ChooseCopy()
    {
        Source = ConfigurationSource.Copy;
    }

    [RelayCommand]
    private void ChooseEmpty()
    {
        Source = ConfigurationSource.Empty;
    }

    [RelayCommand]
    private void ChooseFromFile()
    {
        Source = ConfigurationSource.File;
    }

    [RelayCommand]
    private async Task ChooseFileAsync()
    {
        try
        {
            FileResult? file = await _configurationImportExportService.PickFileAsync();
            if (file == null)
            {
                return;
            }

            IsReadingFile = true;
            ImportFileName = file.FileName;
            ImportedValues = null;
            AllImportNotes = [];
            ImportError = null;

            ConfigurationImport import = await _configurationImportExportService.ImportAsync(file);

            if (import.Values.Count == 0)
            {
                ImportError = new LocalizedString(() => _localizationResourceManager[nameof(CreateConfigurationPopupSR.NewConfigurationFileNoSettings)]);
                return;
            }

            ImportedValues = import.Values;
            AllImportNotes = import.Issues.Select(LocalizeIssue).ToList();

            // A name the user has typed is kept, a name taken from an earlier file is replaced.
            if (string.IsNullOrWhiteSpace(Name) || Name == _nameFromFile)
            {
                _nameFromFile = Path.GetFileNameWithoutExtension(file.FileName);
                Name = _nameFromFile;
            }
        }
        catch (Exception)
        {
            ImportedValues = null;
            AllImportNotes = [];
            ImportError = new LocalizedString(() => _localizationResourceManager[nameof(CreateConfigurationPopupSR.NewConfigurationFileUnreadable)]);
        }
        finally
        {
            IsReadingFile = false;
        }
    }

    [RelayCommand]
    private void ToggleImportNotes()
    {
        AreAllImportNotesShown = !AreAllImportNotesShown;
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private Task CreateAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current,
            new CreateConfigurationResult(Name.Trim(), Source,
                IsCopyChosen ? CopiedConfiguration : null,
                IsFileChosen ? ImportedValues : null));
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current);
    }

    private bool CanChooseCopy()
    {
        return ConfigurationsService.Configurations.Count > 0;
    }

    private bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && (!IsCopyChosen || CopiedConfiguration != null)
            && (!IsFileChosen || ImportedValues?.Count > 0);
    }

    private LocalizedString LocalizeIssue(ConfigurationImportIssue issue)
    {
        string key = issue.Kind switch
        {
            ConfigurationImportIssueKind.UnknownSetting => nameof(CreateConfigurationPopupSR.NewConfigurationFileUnknownSetting),
            ConfigurationImportIssueKind.InvalidValue => nameof(CreateConfigurationPopupSR.NewConfigurationFileInvalidValue),
            _ => nameof(CreateConfigurationPopupSR.NewConfigurationFileMissingValue)
        };

        return new LocalizedString(() => _localizationResourceManager[key, issue.Panel, issue.Key, issue.Value]);
    }

    private void OnDeviceConnectionServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateDevice();
    }

    private void UpdateDevice()
    {
        IsDeviceAvailable = _deviceConnectionService.SelectedDevice != null && _deviceConnectionService.CanSend;

        string detailKey;
        if (!IsDeviceAvailable)
        {
            if (IsDeviceChosen)
            {
                Source = ConfigurationSource.Empty;
            }

            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceUnavailable);
        }
        else if (_deviceConnectionService.SelectedDevice?.Snapshot.LastFullSync != null)
        {
            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceSynced);
        }
        else
        {
            detailKey = nameof(CreateConfigurationPopupSR.NewConfigurationFromDeviceNotSynced);
        }

        DeviceDetail = new LocalizedString(() => _localizationResourceManager[detailKey]);
    }

    public void StopObservingDevice()
    {
        _deviceConnectionService.PropertyChanged -= OnDeviceConnectionServiceChanged;
    }
}
