using Android.Content;

namespace DAC70Setup.Transport.Usb;

internal class UsbBroadcastReceiver : BroadcastReceiver
{
    private readonly Action<Intent> _onReceive;

    public UsbBroadcastReceiver(Action<Intent> onReceive)
    {
        _onReceive = onReceive;
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent != null)
        {
            _onReceive(intent);
        }
    }
}
