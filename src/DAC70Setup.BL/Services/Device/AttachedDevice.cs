using DAC70Setup.Core.Models;

namespace DAC70Setup.BL.Services.Device;

public class AttachedDevice : BindableBase
{
    public AttachedDevice(Dac70Device device, int number)
    {
        _device = device;
        Number = number;
    }

    private Dac70Device _device;
    public Dac70Device Device
    {
        get => _device;
        set => SetProperty(ref _device, value);
    }

    public int Number { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
