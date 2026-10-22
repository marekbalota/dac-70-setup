using System.Text.Json;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.ImportExport;

public class JsonConfigurationSerializer : ConfigurationSerializer
{
    public override string FileExtension => ".json";

    public override void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values)
    {
        var options = new JsonWriterOptions
        {
            Indented = true,
            NewLine = "\n"
        };

        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();

            foreach (var panel in GroupByPanel(values))
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

    protected override IEnumerable<(string Panel, string Key, string? Value)> ReadSettings(Stream stream)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(stream, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(exception.Message, exception);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("The file is not a mapping of panels.");
            }

            foreach (JsonProperty panel in document.RootElement.EnumerateObject())
            {
                if (panel.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (JsonProperty setting in panel.Value.EnumerateObject())
                {
                    if (setting.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }

                    string? text = setting.Value.ValueKind switch
                    {
                        JsonValueKind.String => setting.Value.GetString()?.Trim(),
                        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => setting.Value.GetRawText(),
                        _ => null
                    };

                    yield return (panel.Name.Trim(), setting.Name.Trim(), text);
                }
            }
        }
    }
}
