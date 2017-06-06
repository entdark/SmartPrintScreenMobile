using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ScreenshotPage : ContentPage {
		private string screenshot;
		public string Screenshot {
			get {
				return this.screenshot;
			}
			set {
				if (this.screenshot == value)
					return;
				this.screenshot = value;
				try {
					screenshotImage.Source = new UriImageSource {
						Uri = new Uri(value),
						CachingEnabled = false
					};
					Task.Run(async () => {
						await Task.Delay(5000);
						Device.BeginInvokeOnMainThread(() => {
							connectionStateLabel.IsVisible = false;
							screenshotImage.IsVisible = true;
						});
					});
				} catch {
					screenshotImage.IsVisible = false;
					connectionStateLabel.Text = Strings.ErrorCheckConnection;
					connectionStateLabel.IsVisible = true;
				}
			}
		}
		public ScreenshotPage() {
			InitializeComponent();
		}

		private async void CopyScreenshot(object sender, EventArgs ev) {
			await DisplayAlert(null, Strings.CopiedToClipboardToast, Strings.Ok);
			if (screenshotImage.IsVisible)
				Plugin.Clipboard.CrossClipboard.Current.SetText(this.screenshot);
		}
		private async void DeleteScreenshot(object sender, EventArgs ev) {
			bool delete = await DisplayAlert(null, Strings.DeleteQ, Strings.Ok, Strings.Cancel);
			if (delete) {
				string ss = this.screenshot;
				await Shared.PopAsync(Navigation);
				MessagingCenter.Send(ss, MainPage.DeleteURL);
			}
		}
	}
}