using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Tests.Core.Protocol;

public class Dac70InputSignalTests
{
    // Big-endian width and height, frame rate as whole part plus hundredths, then the interlaced flag
    [Fact]
    public void FromReadResponse_ParsesResolutionFrameRateAndScan()
    {
        var signal = Dac70InputSignal.FromReadResponse(Report.Create(0x13, 0x3E, 0x07, 0x80, 0x04, 0x38, 59, 94, 0x01));

        Assert.Equal(new Dac70InputSignal(1920, 1080, 59.94, true), signal);
        Assert.Equal("1920x1080 59.94i", signal?.ToString());
        Assert.True(signal?.HasSignal);
    }

    [Fact]
    public void FromReadResponse_ProgressiveSignal_EndsWithP()
    {
        var signal = Dac70InputSignal.FromReadResponse(Report.Create(0x13, 0x3E, 0x05, 0x00, 0x02, 0xD0, 50, 0, 0x00));

        Assert.Equal("1280x720 50.00p", signal?.ToString());
    }

    [Fact]
    public void FromReadResponse_NoInput_HasNoSignal()
    {
        var signal = Dac70InputSignal.FromReadResponse(Report.Create(0x13, 0x3E));

        Assert.False(signal?.HasSignal);
    }

    [Fact]
    public void FromReadResponse_WrongHeader_ReturnsNull()
    {
        Assert.Null(Dac70InputSignal.FromReadResponse(Report.Create(0x00, 0x3E, 0x07, 0x80)));
    }

    [Fact]
    public void FromReadResponse_WrongLength_ReturnsNull()
    {
        Assert.Null(Dac70InputSignal.FromReadResponse([0x13, 0x3E, 0x07, 0x80]));
    }
}