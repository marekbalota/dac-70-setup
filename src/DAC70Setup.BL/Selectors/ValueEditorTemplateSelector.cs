using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Selectors;

public class ValueEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate EnumTemplate { get; set; } = null!;

    public DataTemplate RangeTemplate { get; set; } = null!;

    public DataTemplate PositionTemplate { get; set; } = null!;

    public DataTemplate SizeTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item switch
        {
            EnumSettingValue => EnumTemplate,
            RangeSettingValue => RangeTemplate,
            PositionSettingValue => PositionTemplate,
            SizeSettingValue => SizeTemplate,
            _ => throw new NotSupportedException($"No value editor template for setting value type '{item.GetType().Name}'.")
        };
    }
}
