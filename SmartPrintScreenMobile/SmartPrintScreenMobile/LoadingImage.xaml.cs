using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PCLExt.FileStorage;
using PCLExt.FileStorage.Folders;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartPrintScreenMobile {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoadingImage : ContentView {
		private const string CacheFolder = "images";

		private CancellationTokenSource cancelLoading;

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(
			nameof(LoadingImage.Source),
			typeof(string),
			typeof(LoadingImage),
			propertyChanged: OnSourcePropertyChanged
		);
		public static readonly BindableProperty AspectProperty = BindableProperty.Create(
			nameof(LoadingImage.Aspect),
			typeof(Aspect),
			typeof(LoadingImage),
			Aspect.AspectFit
		);
		public static readonly BindableProperty LoadingBackgroundColorProperty = BindableProperty.Create(
			nameof(LoadingImage.LoadingBackgroundColor),
			typeof(Color),
			typeof(LoadingImage),
			Color.Transparent,
			propertyChanged: OnLoadingBackgroundColorPropertyChanged
		);
		public static readonly BindableProperty LoadingPaddingProperty = BindableProperty.Create(
			nameof(LoadingImage.LoadingPadding),
			typeof(Thickness),
			typeof(LoadingImage),
			new Thickness(0.0),
			propertyChanged: OnLoadingPaddingPropertyChanged
		);

		public string Source {
			get => (string)this.GetValue(LoadingImage.SourceProperty);
			set => this.SetValue(LoadingImage.SourceProperty, value);
		}
		public Aspect Aspect {
			get => (Aspect)this.GetValue(LoadingImage.AspectProperty);
			set => this.SetValue(LoadingImage.AspectProperty, value);
		}
		public Color LoadingBackgroundColor {
			get => (Color)this.GetValue(LoadingImage.LoadingBackgroundColorProperty);
			set => this.SetValue(LoadingImage.LoadingBackgroundColorProperty, value);
		}
		public Thickness LoadingPadding {
			get => (Thickness)this.GetValue(LoadingImage.LoadingPaddingProperty);
			set => this.SetValue(LoadingImage.LoadingPaddingProperty, value);
		}

		private bool isLoading;
		public bool IsLoading {
			get => this.isLoading;
			set { this.isLoading = value; this.OnPropertyChanged(); }
		}
		private ImageSource loadingSource;
		public ImageSource LoadingSource {
			get => this.loadingSource;
			set { this.loadingSource = value; this.OnPropertyChanged(); }
		}
		public bool DelayLoading { get; set; }
		public bool IsImageSourceLoading { get; set; }

		public event EventHandler<LoadingImageEventArgs> FinishedLoading;

		public LoadingImage() {
			this.cancelLoading = new CancellationTokenSource();
			this.InitializeComponent();
			this.DelayLoading = false;
			this.Content.BindingContext = this;
		}

		private async Task LoadImage() {
			this.IsLoading = true;
			var stream = await LoadingImage.LoadCache(this.Source);
			if (stream == null) {
				if (this.DelayLoading)
					await Task.Delay(512);
				stream = await HttpService.GetStream(this.Source, cancelLoading.Token);
				bool saved = await LoadingImage.SaveCache(stream, this.Source);
				//reload because the stream gets released.
				if (saved)
					stream = await LoadingImage.LoadCache(this.Source);
			}
			this.LoadingSource = ImageSource.FromStream(() => stream);
			//FromStream can take some time so we are await a bit
			await Task.WhenAny(Task.Run(() => LoadingImageSource()), Task.Delay(5000));
			this.FinishedLoading?.Invoke(this, new LoadingImageEventArgs(stream != null));
			this.IsLoading = false;
		}
		private void LoadingImageSource() {
			while (this.IsImageSourceLoading);
		}
		public void AbortLoading() {
			if (!this.IsLoading)
				return;
			cancelLoading.Cancel();
		}

		private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue) {
			var loadingImage = bindable as LoadingImage;
			string oldSource = oldValue as string;
			string newSource = newValue as string;
			if (newSource != oldSource)
				Task.Run(loadingImage.LoadImage);
		}
		private static void OnLoadingBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue) {
			var loadingImage = bindable as LoadingImage;
			loadingImage.image.BackgroundColor = loadingImage.LoadingBackgroundColor;
		}
		private static void OnLoadingPaddingPropertyChanged(BindableObject bindable, object oldValue, object newValue) {
			var loadingImage = bindable as LoadingImage;
			loadingImage.layout.Padding = loadingImage.LoadingPadding;
		}

		public static string NameFromUri(string uri) {
			return uri.Substring(uri.LastIndexOf('/')+1);
		}
		private static async Task<bool> SaveCache(Stream data, string uri) {
			if (!Settings.CacheScreenshots)
				return false;
			string name = LoadingImage.NameFromUri(uri);
			try {
				var rootFolder = new TempRootFolder();
				var folder = await rootFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
				var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
				byte []buffer = new byte[data.Length];
				data.Read(buffer, 0, buffer.Length);
				using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite)) {
					stream.Write(buffer, 0, buffer.Length);
				}
				return true;
			} catch {
				return false;
			}
		}
		private static async Task<Stream> LoadCache(string uri) {
			string name = LoadingImage.NameFromUri(uri);
			try {
				var rootFolder = new TempRootFolder();
				var folder = await rootFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
				var exists = await folder.CheckExistsAsync(name);
				if (exists == ExistenceCheckResult.FileExists) {
					var file = await folder.GetFileAsync(name);
					return await file.OpenAsync(FileAccess.Read);
				}
			} catch {}
            return null;
        }
		public static async Task<bool> RemoveAllCached() {
			try {
				var rootFolder = new TempRootFolder();
				var folder = await rootFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
				var files = await folder.GetFilesAsync();
				foreach (var file in files) {
					await file.DeleteAsync();
				}
				return true;
			} catch {
				return false;
			}
		}
		public static async Task<bool> RemoveCache(string uri) {
			string name = LoadingImage.NameFromUri(uri);
			try {
				var rootFolder = new TempRootFolder();
				var folder = await rootFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
				var exists = await folder.CheckExistsAsync(name);
				if (exists == ExistenceCheckResult.FileExists) {
					var file = await folder.GetFileAsync(name);
					await file.DeleteAsync();
				}
				return true;
			} catch {
				return false;
			}
		}
		public static async Task<int> CountCache() {
			try {
				var rootFolder = new TempRootFolder();
				var folder = await rootFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
				var files = await folder.GetFilesAsync();
				return files.Count;
			} catch {
				return -1;
			}
		}
	}

	public class LoadingImageEventArgs : EventArgs {
		public bool IsSuccess { get; private set; }
		public LoadingImageEventArgs(bool isSuccess) {
			this.IsSuccess = isSuccess;
		}
	}
}