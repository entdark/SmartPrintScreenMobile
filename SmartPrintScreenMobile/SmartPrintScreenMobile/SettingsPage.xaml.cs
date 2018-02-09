using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage {
		private SettingsItemViewModel startOnBoot, removeShots, cacheScreenshots;

		private ObservableCollection<SettingsItemViewModel> settingsItems;
		public ObservableCollection<SettingsItemViewModel> SettingsItems {
			get => this.settingsItems;
			set { this.settingsItems = value; this.OnPropertyChanged(); }
		}
		public bool HavingScreenshots => LoadingImage.CountCache().Result > 0;

		public SettingsPage() {
			this.InitializeComponent();
			if (Device.RuntimePlatform != Device.iOS) {
				this.SettingsItems = new ObservableCollection<SettingsItemViewModel>(new []{
					this.startOnBoot = new SettingsItemViewModel() {
						Text = Strings.StartOnBoot,
						IsToggled = Settings.StartOnBoot
					},
					this.removeShots = new SettingsItemViewModel() {
						Text = Strings.RemoveShots,
						IsToggled = Settings.RemoveShots
					},
					this.cacheScreenshots = new SettingsItemViewModel() {
						Text = Strings.CacheScreenshots,
						IsToggled = Settings.CacheScreenshots
					}
				});
			} else {
				this.SettingsItems = new ObservableCollection<SettingsItemViewModel>(new[]{
					this.cacheScreenshots = new SettingsItemViewModel() {
						Text = Strings.CacheScreenshots,
						IsToggled = Settings.CacheScreenshots
					}
				});
			}
			this.BindingContext = this;
		}

		private void ItemSelected(object sender, SelectedItemChangedEventArgs ev) {
			if (ev.SelectedItem == null)
				return;
			((ListView)sender).SelectedItem = null;
			var item = (ev.SelectedItem as SettingsItemViewModel);
			if (item == null)
				return;
			item.IsToggled = !item.IsToggled;
		}

		protected override async void OnDisappearing() {
			if (Device.RuntimePlatform != Device.iOS) {
				Settings.StartOnBoot = this.startOnBoot.IsToggled;
				Settings.RemoveShots = this.removeShots.IsToggled;
			}
			if (Settings.CacheScreenshots && !this.cacheScreenshots.IsToggled)
				await LoadingImage.RemoveAllCached();
			Settings.CacheScreenshots = this.cacheScreenshots.IsToggled;
			base.OnDisappearing();
		}
	}
}