using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using AndroidX.Core.Content;

namespace DAC70Setup.Transport.Usb;

internal static class UsbPermissionRequest
{
    public static async Task<bool> RequestAsync(Context context, UsbManager manager, UsbDevice device,
        CancellationToken cancellationToken)
    {
        if (manager.HasPermission(device))
        {
            return true;
        }

        string action = $"{context.PackageName}.USB_PERMISSION";
        var answer = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var receiver = new UsbBroadcastReceiver(
            intent => answer.TrySetResult(intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false)));
        ContextCompat.RegisterReceiver(context, receiver, new IntentFilter(action), ContextCompat.ReceiverNotExported);

        try
        {
            var intent = new Intent(action);
            intent.SetPackage(context.PackageName);

            var flags = OperatingSystem.IsAndroidVersionAtLeast(31)
                ? PendingIntentFlags.Mutable
                : (PendingIntentFlags)0;

            var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, flags);
            manager.RequestPermission(device, pendingIntent);

            using (cancellationToken.Register(() => answer.TrySetCanceled(cancellationToken)))
            {
                return await answer.Task;
            }
        }
        finally
        {
            context.UnregisterReceiver(receiver);
        }
    }
}
