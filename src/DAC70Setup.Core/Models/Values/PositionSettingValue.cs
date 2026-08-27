using System.Buffers.Binary;
using DAC70Setup.Core.Models.Definitions;

namespace DAC70Setup.Core.Models.Values;

public sealed class PositionSettingValue : Dac70SettingValue<PositionSettingDefinition>
{
    public PositionSettingValue(PositionSettingDefinition definition)
        : base(definition)
    {
        _horizontal = definition.HorizontalMinimum;
        _vertical = definition.VerticalMinimum;
    }

    private int _horizontal;
    public int Horizontal
    {
        get => _horizontal;
        set
        {
            if (value < Definition.HorizontalMinimum || value > Definition.HorizontalMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"Horizontal position must be between {Definition.HorizontalMinimum} and {Definition.HorizontalMaximum}.");
            }

            SetProperty(ref _horizontal, value, () => RaisePropertyChanged(nameof(DisplayValueText)));
        }
    }

    private int _vertical;
    public int Vertical
    {
        get => _vertical;
        set
        {
            if (value < Definition.VerticalMinimum || value > Definition.VerticalMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"Vertical position must be between {Definition.VerticalMinimum} and {Definition.VerticalMaximum}.");
            }

            SetProperty(ref _vertical, value, () => RaisePropertyChanged(nameof(DisplayValueText)));
        }
    }

    public override string DisplayValueText => $"{Horizontal}, {Vertical}";

    protected override int ValueByteCount => 6;

    protected override void SetValueBytesInt(ReadOnlySpan<byte> valueBytes)
    {
        Horizontal = BinaryPrimitives.ReadUInt16BigEndian(valueBytes.Slice(2, 2));
        Vertical = valueBytes[5];
    }

    protected override void WriteValueBytes(Span<byte> destination)
    {
        destination[0] = 0x00;
        destination[1] = 0x04;
        BinaryPrimitives.WriteUInt16BigEndian(destination.Slice(2, 2), (ushort)Horizontal);
        destination[4] = 0x00;
        destination[5] = (byte)Vertical;
    }
}
