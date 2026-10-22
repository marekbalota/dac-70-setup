using System.Globalization;
using System.Text;
using DAC70Setup.Core.Models.Values;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace DAC70Setup.BL.Services.ImportExport;

public class YamlConfigurationSerializer : ConfigurationSerializer
{
    public override string FileExtension => ".yaml";

    public override void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values)
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);

        var emitter = new Emitter(writer, new EmitterSettings().WithNewLine("\n"));
        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());

        foreach (var panel in GroupByPanel(values))
        {
            emitter.Emit(new Scalar(panel.Key));
            emitter.Emit(new MappingStart());

            foreach (var value in panel)
            {
                switch (value)
                {
                    case EnumSettingValue enumValue:
                        EmitSetting(emitter, value.Definition.Name, enumValue.SelectedOption.Name);
                        break;
                    case RangeSettingValue rangeValue:
                        EmitSetting(emitter, value.Definition.Name, rangeValue.Value);
                        break;
                    case PositionSettingValue positionValue:
                        EmitSetting(emitter, HorizontalPosition, positionValue.Horizontal);
                        EmitSetting(emitter, VerticalPosition, positionValue.Vertical);
                        break;
                    case SizeSettingValue sizeValue:
                        EmitSetting(emitter, HorizontalSize, sizeValue.Horizontal);
                        EmitSetting(emitter, VerticalSize, sizeValue.Vertical);
                        break;
                }
            }

            emitter.Emit(new MappingEnd());
        }

        emitter.Emit(new MappingEnd());
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());
    }

    protected override IEnumerable<(string Panel, string Key, string? Value)> ReadSettings(Stream stream)
    {
        var yaml = new YamlStream();

        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            yaml.Load(reader);
        }
        catch (Exception exception) when (exception is YamlException or ArgumentException)
        {
            throw new InvalidDataException(exception.Message, exception);
        }

        if (yaml.Documents.Count == 0)
        {
            yield break;
        }

        if (yaml.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new InvalidDataException("The file is not a mapping of panels.");
        }

        foreach (var (panelNode, settingsNode) in root.Children)
        {
            if (panelNode is not YamlScalarNode { Value: { } panel } || settingsNode is not YamlMappingNode settings)
            {
                continue;
            }

            foreach (var (keyNode, valueNode) in settings.Children)
            {
                if (keyNode is not YamlScalarNode { Value: { } key })
                {
                    continue;
                }

                if (valueNode is YamlScalarNode { Style: ScalarStyle.Plain, Value: null or "" or "~" or "null" or "Null" or "NULL" })
                {
                    continue;
                }

                string? text = (valueNode as YamlScalarNode)?.Value?.Trim();

                yield return (panel.Trim(), key.Trim(), text);
            }
        }
    }

    private static void EmitSetting(Emitter emitter, string key, string value)
    {
        emitter.Emit(new Scalar(key));
        emitter.Emit(new Scalar(value));
    }

    private static void EmitSetting(Emitter emitter, string key, int value)
    {
        EmitSetting(emitter, key, value.ToString(CultureInfo.InvariantCulture));
    }
}
