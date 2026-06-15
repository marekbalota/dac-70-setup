using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Core.Models.Values;

public sealed class RangeSettingValue : Dac70SettingValue<RangeSettingDefinition>
{
    public RangeSettingValue(RangeSettingDefinition definition)
        : base(definition)
    {
    }

    private byte _value;
    public byte Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    protected override int ValueByteCount => 1;

    protected override void SetValueBytesInt(ReadOnlySpan<byte> valueBytes)
    {
        Value = valueBytes[0];
    }

    protected override void WriteValueBytes(Span<byte> destination)
    {
        destination[0] = Value;
    }
}
