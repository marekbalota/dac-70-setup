using System.Globalization;
using System.Text;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using static DAC70Setup.BL.Services.ImportExport.IConfigurationSerializer;

namespace DAC70Setup.BL.Services.ImportExport;

public class YamlConfigurationSerializer : IConfigurationSerializer
{
    public string FileExtension => ".yaml";

    public void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values)
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);

        var emitter = new Emitter(writer, new EmitterSettings().WithNewLine("\n"));
        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        emitter.Emit(new MappingStart());

        var orderedValues = Dac70SettingCatalog.All
            .Select(definition => values.FirstOrDefault(value => value.Definition.Key == definition.Key))
            .OfType<Dac70SettingValue>();

        foreach (var panel in orderedValues.GroupBy(value => value.Definition.PanelName))
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
