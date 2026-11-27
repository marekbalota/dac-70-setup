namespace DAC70Setup.Tests.Fakes;

public class FakePreferences : IPreferences
{
    private readonly Dictionary<string, object?> _values = [];

    public bool ContainsKey(string key, string? sharedName = null)
    {
        return _values.ContainsKey(key);
    }

    public void Remove(string key, string? sharedName = null)
    {
        _values.Remove(key);
    }

    public void Clear(string? sharedName = null)
    {
        _values.Clear();
    }

    public void Set<T>(string key, T value, string? sharedName = null)
    {
        _values[key] = value;
    }

    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        return _values.TryGetValue(key, out object? value) ? (T)value! : defaultValue;
    }
}