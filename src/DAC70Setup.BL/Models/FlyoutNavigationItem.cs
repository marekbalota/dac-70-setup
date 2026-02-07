namespace DAC70Setup.BL.Models;

public class FlyoutNavigationItem : BindableBase
{
    public FlyoutNavigationItem(string name, string description, string icon, Uri navigateTo)
    {
        NameKey = name;
        DescriptionKey = description;
        Icon = icon;
        NavigateTo = navigateTo;
    }

    private string _nameKey;
    public string NameKey
    {
        get => _nameKey;
        set => SetProperty(ref _nameKey, value);
    }

    private string _descriptionKey;
    public string DescriptionKey
    {
        get => _descriptionKey;
        set => SetProperty(ref _descriptionKey, value);
    }

    private string _icon;
    public string Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    private Uri _navigateTo;
    public Uri NavigateTo
    {
        get => _navigateTo;
        set => SetProperty(ref _navigateTo, value);
    }
}
