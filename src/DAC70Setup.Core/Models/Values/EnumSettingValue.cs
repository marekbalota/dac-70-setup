using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Core.Models.Values;

public sealed class EnumSettingValue : Dac70SettingValue<EnumSettingDefinition>
{
    public EnumSettingValue(EnumSettingDefinition definition)
        : base(definition)
    {
        _selectedOption = definition.Options[0];
    }

    private EnumOption _selectedOption;
    public EnumOption SelectedOption
    {
        get => _selectedOption;
        set => SetProperty(ref _selectedOption, value);
    }

    protected override int ValueByteCount => Definition.Template.Length;

    protected override void SetValueBytesInt(ReadOnlySpan<byte> valueBytes)
    {
        byte optionValue = valueBytes[Definition.ValueOffset];
        EnumOption? option = Definition.Options.FirstOrDefault(o => o.Value == optionValue);

        if (option is null)
        {
            throw new ArgumentException(
                $"Setting '{Definition.Name}' has no option with value 0x{optionValue:X2}.",
                nameof(valueBytes));
        }

        SelectedOption = option;
    }

    protected override void WriteValueBytes(Span<byte> destination)
    {
        Definition.Template.CopyTo(destination);
        destination[Definition.ValueOffset] = SelectedOption.Value;
    }
}
