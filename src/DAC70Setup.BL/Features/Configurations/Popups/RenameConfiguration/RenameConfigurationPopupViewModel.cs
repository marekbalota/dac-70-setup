using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DAC70Setup.BL.Features.Configurations.Popups.RenameConfiguration;

public partial class RenameConfigurationPopupViewModel : ObservableObject, IQueryAttributable
{
    public const string ConfigurationNameParameter = "ConfigurationName";

    private readonly IPopupService _popupService;

    public RenameConfigurationPopupViewModel(IPopupService popupService)
    {
        _popupService = popupService;

        Name = string.Empty;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
    public partial string Name { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Name = (string)query[ConfigurationNameParameter];
    }

    [RelayCommand(CanExecute = nameof(CanRename))]
    private Task RenameAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current, Name.Trim());
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current);
    }

    private bool CanRename()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}
