namespace DAC70Setup.Core.Models;

/// <param name="SerialNumber">Null when the unit reports none, and on Android until the user has granted access.</param>
public record Dac70Device(string Id, string? SerialNumber);
