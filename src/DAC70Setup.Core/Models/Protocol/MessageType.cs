namespace DAC70Setup.Core.Models.Protocol;

/// <summary>
/// The type byte that follows the 13 3E header. A read request uses the type with 0x20 added (E0, E1).
/// </summary>
public enum MessageType : byte
{
    /// <summary>
    /// Most settings, one value byte each.
    /// </summary>
    C0 = 0xC0,

    /// <summary>
    /// The Proc Amp settings, all read back with one request.
    /// </summary>
    C1 = 0xC1,

    /// <summary>
    /// Settings written as a 6-byte template, which the device cannot read back.
    /// </summary>
    S61 = 0x61
}
