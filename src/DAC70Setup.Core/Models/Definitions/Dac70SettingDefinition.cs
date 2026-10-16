using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.Core.Models.Definitions;

public abstract class Dac70SettingDefinition
{
    protected Dac70SettingDefinition(SettingKey key, PanelType panel, string name)
    {
        Key = key;
        Panel = panel;
        Name = name;
    }

    public SettingKey Key { get; }

    public PanelType Panel { get; }

    public string Name { get; }

    public virtual SettingDependency? Dependency => null;

    public virtual bool IsReadable => false;

    public virtual byte ReadRequestId => Key.Id;

    public abstract Dac70SettingValue CreateValue();

    public Dac70SettingValue CreateValue(ReadOnlySpan<byte> valueBytes)
    {
        Dac70SettingValue value = CreateValue();
        value.SetValueBytes(valueBytes);

        return value;
    }

    public byte[] GetReadPayload()
    {
        var payload = new byte[Dac70UsbIdentity.ReportLength];
        payload[0] = 0x13;
        payload[1] = 0x3E;
        payload[2] = (byte)((byte)Key.Type | 0x20);
        payload[4] = ReadRequestId;

        return payload;
    }

    public Dac70SettingValue? CreateValueFromReadResponse(ReadOnlySpan<byte> response)
    {
        if (!IsReadable || response.Length < Dac70UsbIdentity.ReportLength || response[0] != 0x13 || response[1] != 0x3E)
        {
            return null;
        }

        try
        {
            return CreateValue(GetValueBytesFromReadResponse(response));
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    protected virtual ReadOnlySpan<byte> GetValueBytesFromReadResponse(ReadOnlySpan<byte> response)
    {
        throw new NotSupportedException($"Setting '{Name}' cannot be read from the device.");
    }
}
