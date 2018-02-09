using System.Threading.Tasks;

using Xamarin.Forms;

namespace SmartPrintScreenMobile {
	public static class Shared {
		public const string ScreenshotsListKey = "ScreenshotsList";
		public static async Task PushAsync(INavigation navigation, Page page) {
			if (navigation.NavigationStack.Count <= 1) {
				await navigation.PushAsync(page, true);
			}
		}
		public static async Task PopAsync(INavigation navigation) {
			if (navigation.NavigationStack.Count > 1) {
				await navigation.PopAsync(true);
			}
		}
	}
}
