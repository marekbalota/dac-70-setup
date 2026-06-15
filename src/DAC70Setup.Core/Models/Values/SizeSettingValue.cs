using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Core.Models.Values;

public sealed class SizeSettingValue : Dac70SettingValue<SizeSettingDefinition>
{
    public SizeSettingValue(SizeSettingDefinition definition)
        : base(definition)
    {
        _horizontal = definition.Maximum;
        _vertical = definition.Maximum;
    }

    private int _horizontal;
    public int Horizontal
    {
        get => _horizontal;
        set
        {
            if (value < Definition.Minimum || value > Definition.Maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"Horizontal size must be between {Definition.Minimum} and {Definition.Maximum}.");
            }

            SetProperty(ref _horizontal, value);
        }
    }

    private int _vertical;
    public int Vertical
    {
        get => _vertical;
        set
        {
            if (value < Definition.Minimum || value > Definition.Maximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"Vertical size must be between {Definition.Minimum} and {Definition.Maximum}.");
            }

            SetProperty(ref _vertical, value);
        }
    }

    protected override int ValueByteCount => 6;

    protected override void SetValueBytesInt(ReadOnlySpan<byte> valueBytes)
    {
        Horizontal = valueBytes[2];
        Vertical = valueBytes[3];
    }

    protected override void WriteValueBytes(Span<byte> destination)
    {
        destination[0] = 0x00;
        destination[1] = 0x02;
        destination[2] = (byte)Horizontal;
        destination[3] = (byte)Vertical;
        destination[4] = 0x00;
        destination[5] = 0x00;
    }
}
