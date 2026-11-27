using DAC70Setup.Core.Models;

namespace DAC70Setup.Core.Interfaces;

/// <summary>
/// One way of reaching DAC-70 devices, such as USB.
/// </summary>
public interface IDac70Transport
{
    /// <summary>
    /// Saved in the preferences to restore the user's choice, so it must not change between versions.
    /// </summary>
    string Id { get; }

    string DisplayName { get; }

    /// <summary>
    /// False when this platform cannot use the transport at all, like USB on iOS.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Raised when a device is plugged in or out. It can be raised on a background thread.
    /// </summary>
    event EventHandler? DevicesChanged;

    /// <summary>
    /// Lists the devices attached right now.
    /// </summary>
    IAsyncEnumerable<Dac70Device> DiscoverAsync(CancellationToken cancellationToken);

    /// <exception cref="IOException">The device is gone or could not be opened.</exception>
    /// <exception cref="Exceptions.Dac70PermissionDeniedException">The user did not allow access to the device.</exception>
    Task<IDac70Connection> ConnectAsync(Dac70Device device, CancellationToken cancellationToken);
}
