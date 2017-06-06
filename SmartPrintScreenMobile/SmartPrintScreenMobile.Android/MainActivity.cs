using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using HockeyApp.Android;

namespace SmartPrintScreenMobile.Droid {
	[Activity(
//		Label = "SmartPrintScreen",
		Label = "SmartScreenShot", //SmartPrintScreen -> SmartScreenShot for mobile
		Icon = "@drawable/icon",
		Theme = "@style/MainTheme",
		HardwareAccelerated = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
	)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
		protected override void OnCreate(Bundle bundle) {
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			if (!MainActivity.IsServiceRunning(this, typeof(SmartPrintScreen))) {
				this.StartService(new Intent(this, typeof(SmartPrintScreen)));
			}

			global::Xamarin.Forms.Forms.Init(this, bundle);
			LoadApplication(new App());
		}
		protected override void OnResume() {
			base.OnResume();
			var permissionRead = ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage);
			var permissionWrite = ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage);
			if (permissionRead != Permission.Granted || permissionWrite != Permission.Granted) {
				ActivityCompat.RequestPermissions(
					this,
					new[] {
						Android.Manifest.Permission.ReadExternalStorage,
						Android.Manifest.Permission.WriteExternalStorage
					},
					1337
				);
			}
			CrashManager.Register(this, APIKeys.HockeyAppAndroid);
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
			switch (requestCode) {
			case 1337:
				if (grantResults[0] != Permission.Granted || grantResults[1] != Permission.Granted) {
					SmartPrintScreen.MakeToast(this, Strings.RequestRead);
				}
				break;
			}
		}

		private static bool IsServiceRunning(Context context, Type serviceClass) {
			var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
			var services = manager.GetRunningServices(int.MaxValue);
			foreach (var service in services) {
				string className = service.Service.ClassName;
				Log.Debug("SmartPrintScreen", "Service " + className + " is running");
				if (className.Equals(serviceClass.Name)
					|| (className.Contains(serviceClass.Name) && className.Contains("md5"))) {
					Log.Debug("SmartPrintScreen", "Our service " + serviceClass.Name + " is running amoung " + services.Count);
					return true;
				}
			}
			Log.Debug("SmartPrintScreen", "Our service " + serviceClass.Name + " is not running among " + services.Count);
			return false;
		}
	}
}

