using DAC70Setup.Core.Models.Protocol;
using DAC70Setup.Core.Protocol;
using DAC70Setup.Tests.Fakes;

namespace DAC70Setup.Tests.Core.Protocol;

public class Dac70ReadSessionTests
{
    private static readonly byte[] FirmwareReply = Report.Create(0x13, 0x3E, 0x10, 0x00);
    private static readonly byte[] InputSourceRequest = Report.Create(0x13, 0x3E, 0xE0, 0x00, 0x04);
    private static readonly byte[] AudioSourceRequest = Report.Create(0x13, 0x3E, 0xE0, 0x00, 0x06);

    private readonly FakeDac70Connection _connection = new();

    public Dac70ReadSessionTests()
    {
        _connection.Answer(Dac70FirmwareVersion.GetReadPayload(), FirmwareReply);
    }

    [Fact]
    public async Task ReadAsync_DeviceStillRepeatsPreviousReply_WaitsForTheAnswer()
    {
        byte[] answer = Report.Create(0x13, 0x3E, 0x01);
        _connection.Answer(InputSourceRequest, answer);
        _connection.StaleReadsAfterRequest = 3;
        var session = await Dac70ReadSession.CreateAsync(_connection, TestContext.Current.CancellationToken);

        byte[]? reply = await session.ReadAsync(InputSourceRequest, TestContext.Current.CancellationToken);

        Assert.Equal(answer, reply);
    }

    // Both settings answer 01, so the second reply looks like the first unless a firmware read separates them
    [Fact]
    public async Task ReadAsync_AnswerEqualToPreviousReply_IsStillRecognised()
    {
        byte[] answer = Report.Create(0x13, 0x3E, 0x01);
        _connection.Answer(InputSourceRequest, answer);
        _connection.Answer(AudioSourceRequest, answer);
        var session = await Dac70ReadSession.CreateAsync(_connection, TestContext.Current.CancellationToken);
        await session.ReadFirmwareVersionAsync(TestContext.Current.CancellationToken);
        await session.ReadAsync(InputSourceRequest, TestContext.Current.CancellationToken);

        byte[]? reply = await session.ReadAsync(AudioSourceRequest, TestContext.Current.CancellationToken);

        Assert.Equal(answer, reply);
    }

    // Takes 3 s: the fake never answers, so only the session's reply timeout ends the read
    [Fact]
    public async Task ReadAsync_NoAnswer_ReturnsNull()
    {
        var session = await Dac70ReadSession.CreateAsync(_connection, TestContext.Current.CancellationToken);

        byte[]? reply = await session.ReadAsync(InputSourceRequest, TestContext.Current.CancellationToken);

        Assert.Null(reply);
    }

    [Fact]
    public async Task ReadAsync_CancelledByCaller_ThrowsInsteadOfReturningNull()
    {
        var session = await Dac70ReadSession.CreateAsync(_connection, TestContext.Current.CancellationToken);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.ReadAsync(InputSourceRequest, cancellation.Token));
    }
}