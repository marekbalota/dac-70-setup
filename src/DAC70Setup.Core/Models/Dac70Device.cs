namespace DAC70Setup.Core.Models;

/// <param name="Id">
/// The transport's name for the device, such as a HID device path or an Android device name.
/// It changes with the USB port, and on Android with every replug, so it cannot recognise a device again.
/// </param>
public record Dac70Device(string Id);
