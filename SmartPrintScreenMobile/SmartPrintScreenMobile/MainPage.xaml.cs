using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage {
		public static readonly string AddedURL = "AddedURL";
		public static readonly string AddedURLs = "AddedURLs";
		public static readonly string DeleteURL = "DeleteURL";

		private int columns, rows, fitCount;
		private double cellsize;
		private List<LoadingImage> items;

		private ObservableCollection<string> screenshotsList;
		public ObservableCollection<string> ScreenshotsList {
			get => this.screenshotsList;
			set { this.screenshotsList = value; this.OnPropertyChanged(); }
		}
		private bool tilesViewMode;
		public bool TilesViewMode {
			get => this.tilesViewMode;
			set { this.tilesViewMode = value; this.OnPropertyChanged(); }
		}
		private string viewModeToolbarItemText;
		public string ViewModeToolbarItemText {
			get => this.viewModeToolbarItemText;
			set { this.viewModeToolbarItemText = value; this.OnPropertyChanged(); }
		}
		private bool havingScreenshots;
		public bool HavingScreenshots {
			get => this.havingScreenshots;
			set { this.ShowToolbarItems = this.havingScreenshots = value; this.OnPropertyChanged(); }
		}
		private bool showToolbarItems;
		public bool ShowToolbarItems {
			get => this.showToolbarItems;
			set { this.showToolbarItems = (Device.RuntimePlatform == Device.iOS) ? false : value; this.OnPropertyChanged(); }
		}

		public MainPage() {
			NavigationPage.SetBackButtonTitle(this, "");
			this.fitCount = 0;
			this.items = new List<LoadingImage>();
			this.ScreenshotsList = new ObservableCollection<string>();
			this.TilesViewMode = Settings.TilesViewMode;
			this.ViewModeToolbarItemText = this.TilesViewMode ? Strings.ShowAsList : Strings.ShowAsTiles;
			this.HavingScreenshots = true; //special hack to not remove toolbar items while binding else crash
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
			this.Content.BindingContext = this;
			this.UpdateScreenshotsList();
			this.ScreenshotsList.CollectionChanged += this.ScreenshotsListChanged;
			MessagingCenter.Unsubscribe<string>(this, MainPage.AddedURL);
			MessagingCenter.Subscribe<string>(this, MainPage.AddedURL, message => {
				this.InsertIntoScreenshotsList(message);
			});
			MessagingCenter.Unsubscribe<string>(this, MainPage.AddedURLs);
			MessagingCenter.Subscribe<string []>(this, MainPage.AddedURLs, message => {
				this.InsertIntoScreenshotsList(message);
			});
			MessagingCenter.Unsubscribe<string>(this, MainPage.DeleteURL);
			MessagingCenter.Subscribe<string>(this, MainPage.DeleteURL, message => {
				this.DeleteFromScreenshotsList(message);
			});
		}

		private void ScreenshotsListChanged(object sender, NotifyCollectionChangedEventArgs ev) {
			this.HavingScreenshots = (sender as Collection<string>).Count > 0;
			switch (ev.Action) {
			case NotifyCollectionChangedAction.Add:
				string newItem = ev.NewItems[ev.NewStartingIndex] as string;
				var loadingImage = this.NewLoadingImage(newItem);
				this.items.Insert(0, loadingImage);
				this.layout.Children.Add(loadingImage);
				this.MeasureLayout();
				break;
			case NotifyCollectionChangedAction.Remove:
				string oldItem = ev.OldItems[0] as string;
				var image = this.items.FirstOrDefault(item => item.Source == oldItem);
				this.items.Remove(image);
				this.layout.Children.Remove(image);
				this.MeasureLayout();
				break;
			case NotifyCollectionChangedAction.Reset:
				this.items.Clear();
				this.layout.Children.Clear();
				this.LoadMoreItems(10, this.TilesViewMode);
				break;
			}
		}

		private bool recountingGrid = false;
		private void MeasureLayout() {
			if (this.recountingGrid)
				return;
			this.recountingGrid = true;
			int loadCount = Math.Min(this.screenshotsList.Count, this.fitCount);
			if (this.items.Count < loadCount) {
				this.LoadMoreItems(loadCount, false);
			}
			int count = this.items.Count;
			this.rows = (count / this.columns) + (((count % this.columns) > 0) ? 1 : 0);
			this.layout.HeightRequest = this.cellsize * this.rows;
			double istep = (this.rows > 1) ? (1.0 / (this.rows-1)) : 0.0f,
				jstep = 1.0 / (this.columns-1);
			for (int i = 0, c = 0; i < this.rows && c < count; i++) {
				for (int j = 0; j < this.columns && c < count; j++) {
					AbsoluteLayout.SetLayoutBounds(
						this.items[c],
						new Rectangle(j*jstep, i*istep, this.cellsize, this.cellsize)
					);
					c++;
				}
			}
			this.recountingGrid = false;
		}

		protected override void LayoutChildren(double x, double y, double width, double height) {
			base.LayoutChildren(x, y, width, height);
			this.columns = (int)(width / 159.9);
			this.cellsize = (width-4.0f) / this.columns;
			this.fitCount = this.columns * ((int)(height / this.cellsize)+1);
			this.MeasureLayout();
		}

		private void UpdateScreenshotsList() {
			string list = Settings.ScreenshotsList.Replace(" ", "");
			if (string.IsNullOrEmpty(list)) {
				this.ScreenshotsList.Clear();
				this.HavingScreenshots = this.ScreenshotsList.Count > 0;
				return;
			}
			var screenshotsList = list.Split(new char []{ '\n' }, StringSplitOptions.RemoveEmptyEntries).Reverse();
			this.ScreenshotsList = new ObservableCollection<string>(screenshotsList);
			this.HavingScreenshots = this.ScreenshotsList.Count > 0;
			if (this.TilesViewMode)
				this.LoadMoreItems(10, false);
		}

		private void LoadMoreItems(int count, bool measure = true, bool delayLoading = false) {
			count = count > 0 ? count : 4;
			int itemsCount = this.items.Count;
			int itemsLoadCount = Math.Min(itemsCount + count, this.ScreenshotsList.Count);
			for (int i = itemsCount; i < itemsLoadCount; i++) {
				var loadingImage = this.NewLoadingImage(this.ScreenshotsList[i], delayLoading);
				this.items.Add(loadingImage);
				this.layout.Children.Add(loadingImage);
			}
			if (measure)
				this.MeasureLayout();
		}
		private LoadingImage NewLoadingImage(string source, bool delayLoading = false) {
			var loadingImage = new LoadingImage() {
				Source = source,
				Aspect = Aspect.AspectFill,
				LoadingBackgroundColor = Color.FromHex("#505050"),
				LoadingPadding = 2.0,
				DelayLoading = true
			};
			var tap = new TapGestureRecognizer();
			tap.Tapped += this.CellTapped;
			loadingImage.GestureRecognizers.Add(tap);
			AbsoluteLayout.SetLayoutFlags(loadingImage, AbsoluteLayoutFlags.PositionProportional);
			return loadingImage;
		}

		private async void CellTapped(object sender, EventArgs ev) {
			await Shared.PushAsync(this.Navigation, new ScreenshotPage() {
				Screenshot = (sender as LoadingImage).Source
			});
		}

		private void InsertIntoScreenshotsList(string url) {
			this.ScreenshotsList.Insert(0, url);
			this.HavingScreenshots = this.ScreenshotsList.Count > 0;
		}
		private void InsertIntoScreenshotsList(string []urls) {
			var urlsList = urls.Reverse();
			foreach (var url in urlsList)
				this.ScreenshotsList.Insert(0, url);
			this.HavingScreenshots = this.ScreenshotsList.Count > 0;
		}
		private void DeleteFromScreenshotsList(string url) {
			this.ScreenshotsList.Remove(url);
			this.HavingScreenshots = this.ScreenshotsList.Count > 0;
			var screenshotsList = this.ScreenshotsList.Reverse();
			string list = "";
			foreach (var screenshot in screenshotsList) {
				list += screenshot + "\n";
			}
			Settings.ScreenshotsList = list;
		}

		private void LayoutScrolled(object sender, ScrolledEventArgs ev) {
			var scrollView = sender as ScrollView;
			double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height - 2.0;
			if (scrollingSpace <= ev.ScrollY) {
				int count = (this.columns > 0)
					? (this.columns + ((this.columns - (this.items.Count % this.columns)) % this.columns))
					: 4;
				this.LoadMoreItems(count, delayLoading: true);
			}
		}

		private async void ItemSelected(object sender, SelectedItemChangedEventArgs ev) {
			if (ev.SelectedItem == null)
				return;
			((ListView)sender).SelectedItem = null;
			await Shared.PushAsync(this.Navigation, new ScreenshotPage() {
				Screenshot = (string)ev.SelectedItem
			});
		}

		private async void SettingsClicked(object sender, EventArgs ev) {
			await this.OpenSettingsAsync();
		}
		private async void ClearListClicked(object sender, EventArgs ev) {
			await this.ClearListAsync();
		}
		private async void ToggleViewModeClicked(object sender, EventArgs ev) {
			await this.ToggleViewModeAsync();
		}
		private async void CopyListClicked(object sender, EventArgs ev) {
			await this.CopyListAsync();
		}
		private async void MoreClicked(object sender, EventArgs ev) {
			if (!this.HavingScreenshots) {
				string result2 = await this.DisplayActionSheet(null, Strings.Cancel, null, Strings.Settings);
				if (result2 == Strings.Settings) {
					await OpenSettingsAsync();
				}
				return;
			}
			string result = await this.DisplayActionSheet(
				null,
				Strings.Cancel,
				Strings.ClearList,
				Strings.CopyList,
				ViewModeToolbarItemText,
				Strings.Settings
			);
			if (result == Strings.ClearList) {
				await this.ClearListAsync();
			} else if (result == Strings.CopyList) {
				await this.CopyListAsync();
			} else if (result == ViewModeToolbarItemText) {
				await this.ToggleViewModeAsync();
			} else if (result == Strings.Settings) {
				await this.OpenSettingsAsync();
			}
		}

		private async Task CopyListAsync() {
			if (this.ScreenshotsList.Count <= 0)
				return;
			string list = "";
			foreach (var screenshot in this.ScreenshotsList.ToList()) {
				list += screenshot + "\n";
			}
			list = list.Substring(0, list.Length-1); //remove the last '\n'
			Plugin.Clipboard.CrossClipboard.Current.SetText(list);
			await this.DisplayAlert(null, Strings.CopiedList, Strings.Ok);
		}
		private async Task OpenSettingsAsync() {
			await Shared.PushAsync(this.Navigation, new SettingsPage());
		}
		private async Task ClearListAsync() {
			bool clearList = await this.DisplayAlert(null, Strings.ClearListQ, Strings.Ok, Strings.Cancel);
			if (clearList) {
				Settings.ScreenshotsList = string.Empty;
				this.ScreenshotsList.Clear();
				this.HavingScreenshots = this.ScreenshotsList.Count > 0;
				await LoadingImage.RemoveAllCached();
			}
		}
		private async Task ToggleViewModeAsync() {
			this.TilesViewMode = !this.TilesViewMode;
			if (this.TilesViewMode) {
				if (this.items.Count <= 0) {
					this.LoadMoreItems(10);
				} else {
					this.MeasureLayout();
				}
				await this.scrollView.ScrollToAsync(0.0, 0.0, false);
				this.ViewModeToolbarItemText = Strings.ShowAsList;
			} else {
				this.ViewModeToolbarItemText = Strings.ShowAsTiles;
			}
			Settings.TilesViewMode = this.TilesViewMode;
		}
	}
}