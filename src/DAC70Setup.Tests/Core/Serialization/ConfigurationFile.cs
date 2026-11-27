using System.Text;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using DAC70Setup.Core.Serialization;

namespace DAC70Setup.Tests.Core.Serialization;

/// <summary>
/// Test helper that writes and reads configuration files as text, and builds a configuration to write.
/// </summary>
public static class ConfigurationFile
{
    public static string Write(ConfigurationSerializer serializer, IReadOnlyList<Dac70SettingValue> values)
    {
        using var stream = new MemoryStream();
        serializer.Write(stream, values);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static ConfigurationImport Read(ConfigurationSerializer serializer, string text)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

        return serializer.Read(stream);
    }

    public static List<Dac70SettingValue> CreateEverySetting()
    {
        var values = new List<Dac70SettingValue>();

        foreach (var definition in Dac70SettingCatalog.All)
        {
            var value = definition.CreateValue();

            switch (value)
            {
                case EnumSettingValue enumValue:
                    enumValue.SelectedOption = enumValue.Definition.Options[^1];
                    break;
                case RangeSettingValue rangeValue:
                    rangeValue.Value = (byte)(200 + rangeValue.Definition.Key.Id);
                    break;
                case PositionSettingValue positionValue:
                    positionValue.Horizontal = 300;
                    positionValue.Vertical = 50;
                    break;
                case SizeSettingValue sizeValue:
                    sizeValue.Horizontal = 90;
                    sizeValue.Vertical = 85;
                    break;
            }

            values.Add(value);
        }

        return values;
    }

    public static Dictionary<string, string> Describe(IEnumerable<Dac70SettingValue> values)
    {
        return values.ToDictionary(value => value.Definition.Name, value => value.DisplayValueText);
    }
}