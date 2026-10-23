using DAC70Setup.Core.Interfaces;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.BL.Services.Device;

/// <summary>
/// The DAC-70 repeats its last reply and a reply does not name its request,
/// so equal replies are separated by a firmware read.
/// </summary>
public class Dac70ReadSession
{
    private const int ReplyTimeoutMilliseconds = 3000;
    private const int ComparedReplyBytes = 8;

    private readonly IDac70Connection _connection;

    private byte[]? _currentReply;
    private byte[]? _firmwareReply;
    private bool _isCurrentReplyFirmware;

    private Dac70ReadSession(IDac70Connection connection)
    {
        _connection = connection;
    }

    public static async Task<Dac70ReadSession> CreateAsync(IDac70Connection connection, CancellationToken cancellationToken)
    {
        var session = new Dac70ReadSession(connection);
        session._currentReply = await session.ReadReplyAsync(false, cancellationToken);

        return session;
    }

    public async Task<byte[]?> ReadFirmwareVersionAsync(CancellationToken cancellationToken)
    {
        _firmwareReply = await RequestAsync(Dac70FirmwareVersion.GetReadPayload(), isFirmware: true, cancellationToken);

        return _firmwareReply;
    }

    public async Task<byte[]?> ReadAsync(byte[] payload, CancellationToken cancellationToken)
    {
        if (_firmwareReply != null && !_isCurrentReplyFirmware)
        {
            await RequestAsync(Dac70FirmwareVersion.GetReadPayload(), isFirmware: true, cancellationToken);
        }

        return await RequestAsync(payload, isFirmware: false, cancellationToken);
    }

    private async Task<byte[]?> RequestAsync(byte[] payload, bool isFirmware, CancellationToken cancellationToken)
    {
        await _connection.WriteReportAsync(payload, cancellationToken);

        var reply = await ReadReplyAsync(!isFirmware, cancellationToken);

        if (reply != null)
        {
            _currentReply = reply;
        }

        _isCurrentReplyFirmware = isFirmware && reply != null;

        return reply;
    }

    private async Task<byte[]?> ReadReplyAsync(bool skipFirmwareReply, CancellationToken cancellationToken)
    {
        using var replyCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        replyCancellation.CancelAfter(ReplyTimeoutMilliseconds);

        try
        {
            while (true)
            {
                var report = await _connection.ReadReportAsync(replyCancellation.Token);

                if (!IsSameReply(report, _currentReply) && !(skipFirmwareReply && IsSameReply(report, _firmwareReply)))
                {
                    return report;
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The reply timeout exceeded
            return null;
        }
    }

    private static bool IsSameReply(byte[]? first, byte[]? second)
    {
        return first != null && second != null
            && first.AsSpan(0, ComparedReplyBytes).SequenceEqual(second.AsSpan(0, ComparedReplyBytes));
    }
}
