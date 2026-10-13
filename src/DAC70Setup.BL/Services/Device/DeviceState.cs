namespace DAC70Setup.BL.Services.Device;

public enum DeviceState
{
    NoTransport,
    Disconnected,
    ChoiceRequired,
    SelectionLost,
    PermissionRequired,
    Ready,
    Sending,
    SendCancelled,
    ConnectionFailed,
    SendInterrupted
}
