namespace DAC70Setup.Core.Exceptions;

/// <summary>
/// The user did not allow the app to use the device. Only Android asks.
/// </summary>
public class Dac70PermissionDeniedException : Exception
{
    public Dac70PermissionDeniedException() : base("Access to the DAC-70 was not allowed.")
    {
    }
}
