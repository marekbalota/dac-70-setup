using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;

namespace DAC70Setup.BL.Controls.Confirmation;

public partial class ConfirmationPopupViewModel : ObservableObject, IQueryAttributable
{
    public const string TitleKeyParameter = "TitleKey";
    public const string MessageKeyParameter = "MessageKey";
    public const string MessageArgumentsParameter = "MessageArguments";
    public const string ConfirmTextKeyParameter = "ConfirmTextKey";

    private readonly IPopupService _popupService;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public ConfirmationPopupViewModel(IPopupService popupService, ILocalizationResourceManager localizationResourceManager)
    {
        _popupService = popupService;
        _localizationResourceManager = localizationResourceManager;

        Title = new LocalizedString(() => string.Empty);
        Message = new LocalizedString(() => string.Empty);
        ConfirmText = new LocalizedString(() => string.Empty);
    }

    [ObservableProperty]
    public partial LocalizedString Title { get; private set; }

    [ObservableProperty]
    public partial LocalizedString Message { get; private set; }

    [ObservableProperty]
    public partial LocalizedString ConfirmText { get; private set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var titleKey = (string)query[TitleKeyParameter];
        var messageKey = (string)query[MessageKeyParameter];
        var messageArguments = query.TryGetValue(MessageArgumentsParameter, out var arguments) ? (object[])arguments : [];
        var confirmTextKey = (string)query[ConfirmTextKeyParameter];

        Title = new LocalizedString(() => _localizationResourceManager[titleKey]);
        Message = new LocalizedString(() => _localizationResourceManager[messageKey, messageArguments]);
        ConfirmText = new LocalizedString(() => _localizationResourceManager[confirmTextKey]);
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
