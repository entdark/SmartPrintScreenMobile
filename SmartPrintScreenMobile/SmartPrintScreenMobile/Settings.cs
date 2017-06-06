using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace SmartPrintScreenMobile {
	public static class Settings {
		private static ISettings AppSettings {
			get {
				return CrossSettings.Current;
			}
		}

#region StartOnBoot
		private const string startOnBootKey = "start_on_boot";
		private static readonly bool startOnBootDefault = true;
		public static bool StartOnBoot {
			get {
				return AppSettings.GetValueOrDefault(startOnBootKey, startOnBootDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(startOnBootKey, value);
			}
		}
#endregion
#region RemoveShots
		private const string removeShotsKey = "remove_shots";
		private static readonly bool removeShotsDefault = false;
		public static bool RemoveShots {
			get {
				return AppSettings.GetValueOrDefault(removeShotsKey, removeShotsDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(removeShotsKey, value);
			}
		}
#endregion
#region ScreenshotsList
		private const string screenshotsListKey = "screenshots_list";
		private static readonly string screenshotsListDefault = string.Empty;
		public static string ScreenshotsList {
			get {
				return AppSettings.GetValueOrDefault(screenshotsListKey, screenshotsListDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(screenshotsListKey, value);
			}
		}
#endregion
	}
}