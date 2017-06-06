using System;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage {
		public static readonly string AddedURL = "AddedURL";
		public static readonly string DeleteURL = "DeleteURL";

		public ObservableCollection<string> ScreenshotsList { get; set; }

		public MainPage() {
			NavigationPage.SetHasNavigationBar(this, true);
			InitializeComponent();
			this.ScreenshotsList = new ObservableCollection<string>();
			this.ScreenshotsList.CollectionChanged += (sender, ev) => {
				var items = (ObservableCollection<string>)sender;
				if (items == null || items.Count == 0) {
					emptyListLabel.IsVisible = true;
					screenshotsListView.IsVisible = false;
					clearListToolbarItem.IsVisible = false;
				} else if (emptyListLabel.IsVisible || !screenshotsListView.IsVisible) {
					emptyListLabel.IsVisible = false;
					screenshotsListView.IsVisible = true;
					clearListToolbarItem.IsVisible = true;
				}
			};
			screenshotsListView.ItemsSource = this.ScreenshotsList;
			this.UpdateScreenshotsList();
			MessagingCenter.Unsubscribe<string>(this, MainPage.AddedURL);
			MessagingCenter.Subscribe<string>(this, MainPage.AddedURL, message => {
				this.InsertIntoScreenshotsList(message);
			});
			MessagingCenter.Unsubscribe<string>(this, MainPage.DeleteURL);
			MessagingCenter.Subscribe<string>(this, MainPage.DeleteURL, message => {
				this.DeleteFromScreenshotsList(message);
			});
		}

		private void UpdateScreenshotsList() {
			string list = Settings.ScreenshotsList.Replace(" ", "");
			if (list == null || list == string.Empty) {
				this.ScreenshotsList.Clear();
				return;
			}
			string []screenshotsList = list.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			this.ScreenshotsList.Clear();
			for (int i = screenshotsList.Length-1; i >= 0; i--) {
				this.ScreenshotsList.Add(screenshotsList[i]);
			}
		}
		private void InsertIntoScreenshotsList(string url) {
			this.ScreenshotsList.Insert(0, url);
		}
		private void DeleteFromScreenshotsList(string url) {
			this.ScreenshotsList.Remove(url);
			string list = "";
			for (int i = this.ScreenshotsList.Count-1; i >= 0; i--) {
				list += this.ScreenshotsList[i] + "\n";
			}
			Settings.ScreenshotsList = list;
		}

		private async void ItemSelected(object sender, SelectedItemChangedEventArgs ev) {
			if (ev.SelectedItem == null)
				return;
			((ListView)sender).SelectedItem = null;
			await Shared.PushAsync(Navigation, new ScreenshotPage() {
				Screenshot = (string)ev.SelectedItem
			});
		}
		private async void SettingsClicked(object sender, EventArgs ev) {
			await Shared.PushAsync(Navigation, new SettingsPage());
		}
		private async void ClearListClicked(object sender, EventArgs ev) {
			bool clearList = await DisplayAlert(null, Strings.ClearListQ, Strings.Ok, Strings.Cancel);
			if (clearList) {
				Settings.ScreenshotsList = string.Empty;
				this.ScreenshotsList.Clear();
			}
		}
	}
}