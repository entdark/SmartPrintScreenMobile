using Android.App;
using Android.Content;
using Android.Util;

namespace SmartPrintScreenMobile.Droid {
	[BroadcastReceiver]
	[IntentFilter(new[] { Intent.ActionBootCompleted, "android.intent.action.QUICKBOOT_POWERON" })] //HTC uses QUICKBOOT_POWERON
	public class BootReciever : BroadcastReceiver {
		private const string Tag = "SmartPrintScreen";
		public override void OnReceive(Context context, Intent intent) {
//			Log.Debug(Tag, "BootReciever: OnReceive");
			if (!Settings.StartOnBoot)
				return;
			if ((intent.Action != null)
				&& ((intent.Action == Intent.ActionBootCompleted)
				|| (intent.Action == "android.intent.action.QUICKBOOT_POWERON"))) {
//				Log.Debug(Tag, "BootReciever: Start service");
				context.StartService(new Intent(context, typeof(SmartPrintScreen)));
			} else {
//				Log.Debug(Tag, "BootReciever: Wrong action (" + intent.Action + ")");
			}
		}
	}
}