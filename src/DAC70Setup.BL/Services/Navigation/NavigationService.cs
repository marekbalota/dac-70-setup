namespace DAC70Setup.BL.Services.Navigation;

public class NavigationService : INavigationService
{
    private const string Back = "..";

    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        var query = new ShellNavigationQueryParameters();

        foreach (var parameter in parameters)
        {
            query[parameter.Key] = parameter.Value;
        }

        return Shell.Current.GoToAsync(route, query);
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync(Back);
    }
}
