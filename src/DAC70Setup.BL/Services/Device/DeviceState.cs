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
    Syncing,
    SendCancelled,
    ConnectionFailed,
    SendInterrupted
}
