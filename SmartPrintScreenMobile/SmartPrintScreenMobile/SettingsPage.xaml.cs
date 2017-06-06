using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage {
		private SettingsOptionViewModel startOnBoot, removeShots;
		public SettingsPage() {
			NavigationPage.SetHasNavigationBar(this, true);
			InitializeComponent();
			startOnBoot = new SettingsOptionViewModel() {
				Text = Strings.StartOnBoot,
				IsToggled = Settings.StartOnBoot
			};
			removeShots = new SettingsOptionViewModel() {
				Text = Strings.RemoveShots,
				IsToggled = Settings.RemoveShots
			};
			settingsListView.ItemsSource = new SettingsOptionViewModel[] {
				startOnBoot,
				removeShots
			};
		}

		private void ItemSelected(object sender, SelectedItemChangedEventArgs ev) {
			if (ev.SelectedItem == null)
				return;
			((ListView)sender).SelectedItem = null;
			var option = (ev.SelectedItem as SettingsOptionViewModel);
			if (option == null)
				return;
			option.IsToggled = !option.IsToggled;
		}

		protected override void OnDisappearing() {
			Settings.StartOnBoot = startOnBoot.IsToggled;
			Settings.RemoveShots = removeShots.IsToggled;
			base.OnDisappearing();
		}
	}
}