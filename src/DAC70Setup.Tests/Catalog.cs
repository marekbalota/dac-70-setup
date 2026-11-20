using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Tests;

/// <summary>
/// Test helper that finds settings and their options in <see cref="Dac70SettingCatalog"/> by name,
/// so test cases can name a setting instead of building its <c>SettingKey</c>.
/// </summary>
public static class Catalog
{
    public static T Get<T>(string name)
        where T : Dac70SettingDefinition
    {
        return (T)Dac70SettingCatalog.All.Single(definition => definition.Name == name);
    }

    public static EnumOption GetOption(EnumSettingDefinition definition, string optionName)
    {
        return definition.Options.Single(option => option.Name == optionName);
    }
}