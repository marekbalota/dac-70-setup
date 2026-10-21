using System.Text.Json;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using static DAC70Setup.BL.Services.ImportExport.IConfigurationSerializer;

namespace DAC70Setup.BL.Services.ImportExport;

public class JsonConfigurationSerializer : IConfigurationSerializer
{
    public string FileExtension => ".json";

    public void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values)
    {
        var options = new JsonWriterOptions
        {
            Indented = true,
            NewLine = "\n"
        };

        var orderedValues = Dac70SettingCatalog.All
            .Select(definition => values.FirstOrDefault(value => value.Definition.Key == definition.Key))
            .OfType<Dac70SettingValue>();

        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();

            foreach (var panel in orderedValues.GroupBy(value => value.Definition.PanelName))
            {
                writer.WriteStartObject(panel.Key);

                foreach (var value in panel)
                {
                    switch (value)
                    {
                        case EnumSettingValue enumValue:
                            writer.WriteString(value.Definition.Name, enumValue.SelectedOption.Name);
                            break;
                        case RangeSettingValue rangeValue:
                            writer.WriteNumber(value.Definition.Name, rangeValue.Value);
                            break;
                        case PositionSettingValue positionValue:
                            writer.WriteNumber(HorizontalPosition, positionValue.Horizontal);
                            writer.WriteNumber(VerticalPosition, positionValue.Vertical);
                            break;
                        case SizeSettingValue sizeValue:
                            writer.WriteNumber(HorizontalSize, sizeValue.Horizontal);
                            writer.WriteNumber(VerticalSize, sizeValue.Vertical);
                            break;
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        stream.WriteByte((byte)'\n');
    }
}
