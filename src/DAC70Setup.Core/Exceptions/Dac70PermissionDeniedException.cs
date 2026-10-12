namespace DAC70Setup.Core.Exceptions;

public class Dac70PermissionDeniedException : Exception
{
    public Dac70PermissionDeniedException() : base("Access to the DAC-70 was not allowed.")
    {
    }
}
