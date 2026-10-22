using System.Globalization;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.ImportExport;

public abstract class ConfigurationSerializer
{
    protected const string HorizontalPosition = "Horizontal Position";
    protected const string VerticalPosition = "Vertical Position";
    protected const string HorizontalSize = "Horizontal Size";
    protected const string VerticalSize = "Vertical Size";

    private static readonly Dictionary<string, Dictionary<string, Dac70SettingDefinition>> Definitions = Dac70SettingCatalog.All
        .GroupBy(definition => definition.PanelName)
        .ToDictionary(panel => panel.Key,
            panel => panel
                .SelectMany(definition =>
                {
                    string[] keys = definition switch
                    {
                        PositionSettingDefinition => [HorizontalPosition, VerticalPosition],
                        SizeSettingDefinition => [HorizontalSize, VerticalSize],
                        _ => [definition.Name]
                    };

                    return keys.Select(key => (Key: key, Definition: definition));
                })
                .ToDictionary(setting => setting.Key, setting => setting.Definition, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);

    public abstract string FileExtension { get; }

    public abstract void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values);

    protected abstract IEnumerable<(string Panel, string Key, string? Value)> ReadSettings(Stream stream);

    protected static IEnumerable<IGrouping<string, Dac70SettingValue>> GroupByPanel(IReadOnlyList<Dac70SettingValue> values)
    {
        return Dac70SettingCatalog.All
            .Select(definition => values.FirstOrDefault(value => value.Definition.Key == definition.Key))
            .OfType<Dac70SettingValue>()
            .GroupBy(value => value.Definition.PanelName);
    }

    public ConfigurationImport Read(Stream stream)
    {
        var values = new Dictionary<Dac70SettingDefinition, Dac70SettingValue>();
        var readPairKeys = new HashSet<string>();
        var issues = new List<ConfigurationImportIssue>();

        foreach (var (panel, key, value) in ReadSettings(stream))
        {
            if (!Definitions.TryGetValue(panel, out Dictionary<string, Dac70SettingDefinition>? panelDefinitions)
                || !panelDefinitions.TryGetValue(key, out Dac70SettingDefinition? definition))
            {
                issues.Add(new ConfigurationImportIssue(ConfigurationImportIssueKind.UnknownSetting, panel, key, value ?? string.Empty));
                continue;
            }

            if (value == null || !TrySetValue(definition, key, value, values, readPairKeys))
            {
                issues.Add(new ConfigurationImportIssue(ConfigurationImportIssueKind.InvalidValue, panel, key, value ?? "…"));
            }
        }

        AddFilledHalfIssues(values.Values, readPairKeys, issues);

        return new ConfigurationImport(values.Values.ToList(), issues);
    }

    private static bool TrySetValue(Dac70SettingDefinition definition, string key, string value,
        Dictionary<Dac70SettingDefinition, Dac70SettingValue> values, HashSet<string> readPairKeys)
    {
        switch (definition)
        {
            case EnumSettingDefinition enumDefinition:
                {
                    EnumOption? option = enumDefinition.Options.FirstOrDefault(option =>
                        string.Equals(option.Name, value, StringComparison.OrdinalIgnoreCase));

                    if (option == null)
                    {
                        return false;
                    }

                    var enumValue = (EnumSettingValue)enumDefinition.CreateValue();
                    enumValue.SelectedOption = option;
                    values[definition] = enumValue;

                    return true;
                }
            case RangeSettingDefinition rangeDefinition:
                {
                    if (!TryParseNumber(value, rangeDefinition.Minimum, rangeDefinition.Maximum, out int number))
                    {
                        return false;
                    }

                    var rangeValue = (RangeSettingValue)rangeDefinition.CreateValue();
                    rangeValue.Value = (byte)number;
                    values[definition] = rangeValue;

                    return true;
                }
            case PositionSettingDefinition positionDefinition:
                {
                    bool isHorizontal = string.Equals(key, HorizontalPosition, StringComparison.OrdinalIgnoreCase);

                    bool isValid = isHorizontal
                        ? TryParseNumber(value, positionDefinition.HorizontalMinimum, positionDefinition.HorizontalMaximum, out int number)
                        : TryParseNumber(value, positionDefinition.VerticalMinimum, positionDefinition.VerticalMaximum, out number);

                    if (!isValid)
                    {
                        return false;
                    }

                    var positionValue = (PositionSettingValue)GetOrCreateValue(definition, values);

                    if (isHorizontal)
                    {
                        positionValue.Horizontal = number;
                    }
                    else
                    {
                        positionValue.Vertical = number;
                    }

                    readPairKeys.Add(isHorizontal ? HorizontalPosition : VerticalPosition);

                    return true;
                }
            case SizeSettingDefinition sizeDefinition:
                {
                    bool isHorizontal = string.Equals(key, HorizontalSize, StringComparison.OrdinalIgnoreCase);

                    if (!TryParseNumber(value, sizeDefinition.Minimum, sizeDefinition.Maximum, out int number))
                    {
                        return false;
                    }

                    var sizeValue = (SizeSettingValue)GetOrCreateValue(definition, values);

                    if (isHorizontal)
                    {
                        sizeValue.Horizontal = number;
                    }
                    else
                    {
                        sizeValue.Vertical = number;
                    }

                    readPairKeys.Add(isHorizontal ? HorizontalSize : VerticalSize);

                    return true;
                }
            default:
                return false;
        }
    }

    private static Dac70SettingValue GetOrCreateValue(Dac70SettingDefinition definition,
        Dictionary<Dac70SettingDefinition, Dac70SettingValue> values)
    {
        if (!values.TryGetValue(definition, out Dac70SettingValue? value))
        {
            value = definition.CreateValue();
            values[definition] = value;
        }

        return value;
    }

    // A pair with only one half in the file keeps the app's default for the other half.
    private static void AddFilledHalfIssues(IEnumerable<Dac70SettingValue> values, HashSet<string> readPairKeys,
        List<ConfigurationImportIssue> issues)
    {
        foreach (var value in values)
        {
            (string Key, int Number)[] halves = value switch
            {
                PositionSettingValue position => [(HorizontalPosition, position.Horizontal), (VerticalPosition, position.Vertical)],
                SizeSettingValue size => [(HorizontalSize, size.Horizontal), (VerticalSize, size.Vertical)],
                _ => []
            };

            foreach (var (key, number) in halves.Where(half => !readPairKeys.Contains(half.Key)))
            {
                issues.Add(new ConfigurationImportIssue(ConfigurationImportIssueKind.MissingValueFilled, value.Definition.PanelName, key,
                    number.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }

    private static bool TryParseNumber(string text, int minimum, int maximum, out int number)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
            && number >= minimum
            && number <= maximum;
    }
}
