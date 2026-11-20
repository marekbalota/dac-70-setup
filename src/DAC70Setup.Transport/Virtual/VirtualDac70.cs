using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Transport.Virtual;

/// <summary>
/// Emulated DAC-70. Like the real device it keeps its settings between connections,
/// answers only read requests, and repeats its last reply until it sends the next one.
/// </summary>
internal class VirtualDac70
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private const byte WriteC0 = 0xC0;
    private const byte WriteC1 = 0xC1;
    private const byte ReadC0 = 0xE0;
    private const byte ReadC1 = 0xE1;
    private const byte QueryInputSignal = 0x62;

    private const byte FactoryResetId = 0x03;
    private const byte FirmwareRequestId = 0x05;
    private const byte ProcAmpRequestId = 0x02;

    // Placeholders until the factory values are recorded from a real device
    private static readonly Dictionary<byte, byte> FactoryC0Values = new()
    {
        [0x02] = 0x0F, // Output Format: 1920x1080 60.00p
        [0x04] = 0x01, // Input Source: HDMI
        [0x06] = 0x01, // Audio Source: Internal
        [0x07] = 0x00, // Aspect Ratio: Keep Aspect Ratio
        [0x0A] = 0x01, // HDMI Output Color: YUV 444
        [0x0B] = 0x00, // Audio Mute: OFF
        [0x0C] = 0x00, // HDMI Input EQ: MIDDLE
        [0x0F] = 0x00, // Video Output Range: Limit Range
        [0x10] = 0x00, // Audio Level: SMTPE(USA)
        [0x13] = 0x00, // Edge Enhance: Disable
        [0x15] = 0x00  // OSD Hide Mode: Disable
    };

    private static readonly Dictionary<byte, byte> FactoryC1Values = new()
    {
        [0x02] = 0x80, // Brightness
        [0x03] = 0x80, // Contrast
        [0x04] = 0x80, // Hue
        [0x05] = 0x80  // Saturation
    };

    // Firmware 1.0.0.0
    private static readonly byte[] FirmwareReply = [0x00, 0x10];

    private readonly byte[] _inputSignalReply;

    private Dictionary<byte, byte> _c0Values = new(FactoryC0Values);
    private Dictionary<byte, byte> _c1Values = new(FactoryC1Values);
    private byte[] _lastReply = new byte[Dac70UsbIdentity.ReportLength];

    public VirtualDac70(byte[] inputSignalReply)
    {
        _inputSignalReply = inputSignalReply;
    }

    public async Task<byte[]> GetLastReplyAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return _lastReply.ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Type 61 writes are accepted and dropped: the panel cannot read them back, so nothing could observe them
    public async Task ReceiveAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken)
    {
        byte[] bytes = report.ToArray();

        if (bytes.Length != Dac70UsbIdentity.ReportLength || bytes[0] != 0x13 || bytes[1] != 0x3E)
        {
            return;
        }

        byte type = bytes[2];
        byte id = bytes[3];

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            switch (type)
            {
                case WriteC0 when id == FactoryResetId && bytes[4] == 0x01:
                    ResetToFactoryValues();
                    break;
                case WriteC0 when FactoryC0Values.ContainsKey(id):
                    _c0Values[id] = bytes[4];
                    break;
                case WriteC1 when FactoryC1Values.ContainsKey(id):
                    _c1Values[id] = bytes[4];
                    break;
                case ReadC0:
                    ReplyToC0Read(bytes[4]);
                    break;
                case ReadC1 when bytes[4] == ProcAmpRequestId:
                    ReplyToProcAmpRead();
                    break;
                case QueryInputSignal when id == 0x01 && bytes[5] == 0x14:
                    Reply(_inputSignalReply);
                    break;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ReplyToC0Read(byte requestId)
    {
        if (requestId == FirmwareRequestId)
        {
            Reply(FirmwareReply);
        }
        else if (_c0Values.TryGetValue(requestId, out byte value))
        {
            Reply([value]);
        }
    }

    // One reply carries every Proc Amp value, each at the byte matching its setting id
    private void ReplyToProcAmpRead()
    {
        var reply = new byte[Dac70UsbIdentity.ReportLength - 2];

        foreach (var (id, value) in _c1Values)
        {
            reply[id - 2] = value;
        }

        Reply(reply);
    }

    private void Reply(ReadOnlySpan<byte> afterHeader)
    {
        var reply = new byte[Dac70UsbIdentity.ReportLength];
        reply[0] = 0x13;
        reply[1] = 0x3E;
        afterHeader.CopyTo(reply.AsSpan(2));

        _lastReply = reply;
    }

    private void ResetToFactoryValues()
    {
        _c0Values = new Dictionary<byte, byte>(FactoryC0Values);
        _c1Values = new Dictionary<byte, byte>(FactoryC1Values);
    }
}