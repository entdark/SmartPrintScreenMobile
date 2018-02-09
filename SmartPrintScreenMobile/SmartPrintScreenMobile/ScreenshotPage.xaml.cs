using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ScreenshotPage : ContentPage {
		private string screenshot;
		public string Screenshot {
			get => this.screenshot;
			set { this.screenshot = value; this.OnPropertyChanged(); }
		}

		private bool isValidScreenshot;
		public bool IsValidScreenshot {
			get => this.isValidScreenshot;
			set { this.isValidScreenshot = value; this.OnPropertyChanged(); }
		}

		public ScreenshotPage() {
			this.InitializeComponent();
			switch (Device.RuntimePlatform) {
			case Device.iOS:
				foreach (var toolbarItem in this.ToolbarItems.ToList()) {
					if (toolbarItem != this.moreToolbarItem)
						this.ToolbarItems.Remove(toolbarItem);
				}
				break;
			default:
				foreach (var toolbarItem in this.ToolbarItems.ToList()) {
					if (toolbarItem == this.moreToolbarItem) {
						this.ToolbarItems.Remove(toolbarItem);
						break;
					}
				}
				break;
			}
			this.IsValidScreenshot = true;
			this.BindingContext = this;
		}

		protected override void OnDisappearing() {
			base.OnDisappearing();
			this.loadingImage.AbortLoading();
		}

		private void FinishedLoading(object sender, LoadingImageEventArgs ev) {
			this.IsValidScreenshot = ev.IsSuccess;
		}
		private async void CopyScreenshotClicked(object sender, EventArgs ev) {
			await this.CopyScreenshotAsync();
		}
		private async void DeleteScreenshotClicked(object sender, EventArgs ev) {
			await this.DeleteScreenshotAsync();
		}
		private async void MoreClicked(object sender, EventArgs ev) {
			string result = await this.DisplayActionSheet(
				null,
				Strings.Cancel,
				Strings.Delete,
				Strings.CopyURL
			);
			if (result == Strings.CopyURL) {
				await this.CopyScreenshotAsync();
			} else if (result == Strings.Delete) {
				await this.DeleteScreenshotAsync();
			}
		}

		private async Task CopyScreenshotAsync() {
			await this.DisplayAlert(null, Strings.CopiedToClipboardToast, Strings.Ok);
			if (this.screenshot != null)
				Plugin.Clipboard.CrossClipboard.Current.SetText(this.screenshot);
		}
		private async Task DeleteScreenshotAsync() {
			bool delete = await this.DisplayAlert(null, Strings.DeleteQ, Strings.Ok, Strings.Cancel);
			if (delete) {
				string ss = this.screenshot;
				await Shared.PopAsync(this.Navigation);
				MessagingCenter.Send(ss, MainPage.DeleteURL);
				await LoadingImage.RemoveCache(ss);
			}
		}
	}
}