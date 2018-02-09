using Xamarin.Forms;

namespace SmartPrintScreenMobile {
	public partial class App : Application {
		public App() {
			if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android) {
				var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
				Strings.Culture = ci; // set the RESX for resource localization
				DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods
			}
			MainPage = new NavigationPage(new MainPage()) {
				BarTextColor = Color.FromHex("FFFFFF")
			};
		}

		protected override void OnStart() {
			// Handle when your app starts
		}

		protected override void OnSleep() {
			// Handle when your app sleeps
		}

		protected override void OnResume() {
			// Handle when your app resumes
		}
	}
}
