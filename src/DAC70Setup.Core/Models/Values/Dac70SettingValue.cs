using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Core.Models.Values;

public abstract class Dac70SettingValue : NotifyPropertyChangedBase
{
    protected Dac70SettingValue(Dac70SettingDefinition definition)
    {
        Definition = definition;
    }

    public virtual Dac70SettingDefinition Definition { get; }

    public abstract string DisplayValueText { get; }

    public byte[] GetPayload()
    {
        var payload = new byte[Dac70UsbIdentity.ReportLength];
        payload[0] = 0x13;
        payload[1] = 0x3E;
        payload[2] = (byte)Definition.Key.Type;
        payload[3] = Definition.Key.Id;
        WriteValueBytes(payload.AsSpan(4, ValueByteCount));

        return payload;
    }

    public byte[] GetValueBytes()
    {
        var valueBytes = new byte[ValueByteCount];
        WriteValueBytes(valueBytes);

        return valueBytes;
    }

    public void SetValueBytes(ReadOnlySpan<byte> valueBytes)
    {
        if (valueBytes.Length != ValueByteCount)
        {
            throw new ArgumentException(
                $"Setting '{Definition.Name}' expects {ValueByteCount} value byte(s), got {valueBytes.Length}.",
                nameof(valueBytes));
        }

        SetValueBytesInt(valueBytes);
    }

    protected abstract void SetValueBytesInt(ReadOnlySpan<byte> valueBytes);

    protected abstract int ValueByteCount { get; }

    protected abstract void WriteValueBytes(Span<byte> destination);
}
