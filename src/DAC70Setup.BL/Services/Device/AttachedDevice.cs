using CommunityToolkit.Mvvm.ComponentModel;
using DAC70Setup.Core.Models;

namespace DAC70Setup.BL.Services.Device;

public partial class AttachedDevice : ObservableObject
{
    public AttachedDevice(Dac70Device device, int number)
    {
        Device = device;
        Number = number;
    }

    public Dac70Device Device { get; }

    public int Number { get; }

    public DeviceSnapshot Snapshot { get; } = new();

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
