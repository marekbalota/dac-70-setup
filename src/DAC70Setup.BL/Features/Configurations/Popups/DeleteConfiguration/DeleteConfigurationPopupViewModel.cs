using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration.Resources;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Features.Configurations.Popups.DeleteConfiguration;

public partial class DeleteConfigurationPopupViewModel : ObservableObject, IQueryAttributable
{
    public const string ConfigurationNameParameter = "ConfigurationName";

    private readonly IPopupService _popupService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public DeleteConfigurationPopupViewModel(IPopupService popupService,
        ILocalizationResourceManager localizationResourceManager)
    {
        _popupService = popupService;
        _localizationResourceManager = localizationResourceManager;

        Message = new LocalizedString(() => string.Empty);
    }

    [ObservableProperty]
    public partial LocalizedString Message { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        string configurationName = (string)query[ConfigurationNameParameter];

        Message = new LocalizedString(() => _localizationResourceManager[
            nameof(DeleteConfigurationPopupSR.DeleteConfigurationConfirmMessage), configurationName]);
    }

    [RelayCommand]
    private Task ConfirmAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current, true);
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return _popupService.ClosePopupAsync(Shell.Current, false);
    }
}
