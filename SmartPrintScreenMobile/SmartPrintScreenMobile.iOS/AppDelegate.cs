using System;
using System.Linq;

using Foundation;
using HockeyApp.iOS;
using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace SmartPrintScreenMobile.iOS {
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
			UINavigationBar.Appearance.BarTintColor = Color.FromHex("43A047").ToUIColor();
			UINavigationBar.Appearance.TintColor = Color.FromHex("FFFFFF").ToUIColor();
			UINavigationBar.Appearance.BackgroundColor = Color.FromHex("303030").ToUIColor();
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes() {
				TextColor = Color.FromHex("FFFFFF").ToUIColor()
			});
			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);

			var manager = BITHockeyManager.SharedHockeyManager;
			manager.Configure(APIKeys.HockeyAppiOS);
			manager.CrashManager.CrashManagerStatus = BITCrashManagerStatus.AutoSend;
			manager.StartManager();

			global::Xamarin.Forms.Forms.Init();
			this.LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}

		public override void OnActivated(UIApplication uiApplication) {
			this.LoadPendingScreenshotsList();
			base.OnActivated(uiApplication);
		}

		private void LoadPendingScreenshotsList() {
			var userDefaults = new NSUserDefaults("group.com.vlbor.SmartPrintScreen", NSUserDefaultsType.SuiteName);
			userDefaults.Synchronize();
			string screenshotsList = userDefaults.StringForKey(Shared.ScreenshotsListKey);
			if (string.IsNullOrEmpty(screenshotsList))
				return;
			Settings.ScreenshotsList += screenshotsList;
			string list = screenshotsList.Replace(" ", "");
			userDefaults.SetString("", Shared.ScreenshotsListKey);
			userDefaults.Synchronize();
			if (string.IsNullOrEmpty(list))
				return;
			var urls = list.Split(new char []{ '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
			MessagingCenter.Send(urls, MainPage.AddedURLs);
		}
	}
}
