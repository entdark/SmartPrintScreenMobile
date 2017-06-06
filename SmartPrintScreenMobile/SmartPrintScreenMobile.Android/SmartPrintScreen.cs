using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;

using Xamarin.Forms;

using HockeyApp.Android;
using Android.Database;
using Android.Net;
using Android.Provider;
using Android.Support.V4.Content;

namespace SmartPrintScreenMobile.Droid {
	[Service]
	public class SmartPrintScreen : Service {
		private const string Tag = "SmartPrintScreen";

		private static string eStorage = Android.OS.Environment.ExternalStorageDirectory.ToString();
		private static string sep = Java.IO.File.Separator;
		//Folder that is supposed to contain "Screenshots" folder
		private static string[] screenshotsFolder = {
		eStorage,
		eStorage + sep + Android.OS.Environment.DirectoryPictures,
		eStorage + sep + Android.OS.Environment.DirectoryDcim,
		eStorage + sep + "ScreenCapture"}; //old Samsung

//		private FileObserver[] fileObserver;
//		private FileObserver[] fileObserverLvlUp;
		private ContentObserver contentObserver;

		public override void OnCreate() {
			Log.Debug(Tag, "SmartPrintScreen: OnCreate");
			base.OnCreate();
			CrashManager.Register(this, APIKeys.HockeyAppAndroid);
		}
		public override void OnDestroy() {
			Log.Debug(Tag, "SmartPrintScreen: OnDestroy");
/*			foreach (var fo in this.fileObserver) {
				fo?.StopWatching();
				fo?.Dispose();
			}
			foreach (var fo in this.fileObserverLvlUp) {
				fo?.StopWatching();
				fo?.Dispose();
			}*/
			if (this.contentObserver != null) {
				this.ContentResolver.UnregisterContentObserver(this.contentObserver);
				this.contentObserver.Dispose();
			}
			this.handler?.Dispose();
			base.OnDestroy();
			//I hope it's not that bad
/*			Task.Run(() => {
				this.StartService(new Intent(this, typeof(SmartPrintScreen)));
			});*/
		}

		public override IBinder OnBind(Intent intent) {
			return null;
		}
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
			Log.Debug(Tag, "SmartPrintScreen: OnStartCommand");
//			this.fileObserver = new FileObserver[screenshotsFolder.Length];
//			this.fileObserverLvlUp = new FileObserver[screenshotsFolder.Length];
			this.contentObserver = new UploadContentObserver(this.handler, this, this.ContentResolver);
			this.ContentResolver.RegisterContentObserver(MediaStore.Images.Media.ExternalContentUri, true, this.contentObserver);
/*			for (int i = 0; i < screenshotsFolder.Length; i++) {
				string ssFolder = screenshotsFolder[i] + sep + "Screenshots";
				bool found = false;
				if (!(new Java.IO.File(ssFolder)).Exists()) {
					if (!(new Java.IO.File(screenshotsFolder[i])).Exists()) {
						continue;
					}
					Log.Debug(Tag, "SmartPrintScreen: StartWatchingFileObserver (" + i + ") at " + screenshotsFolder[i]);
					fileObserverLvlUp[i] = new StartWatchingFileObserver(screenshotsFolder[i], fileObserver[i]);
					fileObserverLvlUp[i].StartWatching();
				} else {
					found = true;
				}
				Log.Debug(Tag, "SmartPrintScreen: UploadFileObserver (" + i + ") at " + ssFolder);
				fileObserver[i] = new UploadFileObserver(ssFolder, this);
				if (found) {
					fileObserver[i].StartWatching();
				}
			}*/
			return StartCommandResult.Sticky;
		}

		private static void Upload(SmartPrintScreen service, string screenshotFile) {
			var permissionCheck = ContextCompat.CheckSelfPermission(service, Android.Manifest.Permission.ReadExternalStorage);
			if (permissionCheck != Android.Content.PM.Permission.Granted) {
				SmartPrintScreen.MakeToast(service, Strings.RequestRead);
				return;
			}
			using (var opt = new BitmapFactory.Options()) {
				opt.InDither = true;
				opt.InPreferredConfig = Bitmap.Config.Argb8888;
				var bitmap = BitmapFactory.DecodeFile(screenshotFile, opt);
				if (bitmap != null) {
					bool disposed = false;
					Task.Run(async () => {
						string url = await SmartPrintScreen.UploadToImgurTask(bitmap, screenshotFile);
						bitmap.Dispose();
						disposed = true;
						SmartPrintScreen.PostUpload(service, url, screenshotFile);
					}).ContinueWith((t) => {
						if (!disposed)
							bitmap.Dispose();
						service.handler.Post(() => {
							ExceptionHandler.SaveManagedException(Java.Lang.Throwable.FromException(t.Exception), Java.Lang.Thread.CurrentThread(), null);
						});
					}, TaskContinuationOptions.OnlyOnFaulted);
				}
			}
		}
		private static async Task<string> UploadToImgurTask(Bitmap bitmap, string screenshotFile) {
			try {
				using (var w = new WebClient()) {
					w.Headers.Add("Authorization", "Client-ID " + APIKeys.ImgurClientID);
					System.Collections.Specialized.NameValueCollection Keys = new System.Collections.Specialized.NameValueCollection();
					using (MemoryStream ms = new MemoryStream()) {
						bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
						byte[] byteImage = ms.ToArray();
						Keys.Add("image", Convert.ToBase64String(byteImage));
					}
					byte[] responseArray = await w.UploadValuesTaskAsync("https://api.imgur.com/3/image", Keys);
					string result = Encoding.ASCII.GetString(responseArray);
					Regex reg = new Regex("link\":\"(.*?)\"");
					Match match = reg.Match(result);
					string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
					return url;
				}
			} catch (Exception exception) {
				return null;
			}
		}
		private Handler handler = new Handler();
		private static void PostUpload(SmartPrintScreen service, string url, string screenshotFile) {
			Log.Debug(Tag, "SmartPrintScreen: PostUpload " + url);
			if (url != null) {
				Log.Debug(Tag, "SmartPrintScreen: PostUpload prepared showing toast");
//				Device.BeginInvokeOnMainThread(() => {
					service.handler.Post(() => {
						Log.Debug(Tag, "SmartPrintScreen: PostUpload got from the clipboard");
						Plugin.Clipboard.CrossClipboard.Current.SetText(url);
						SmartPrintScreen.MakeToast(service, Strings.CopiedToClipboardToast + ":\n" + url);
						MessagingCenter.Send(url, MainPage.AddedURL);
					});
//				});
				//we reached here, so it got uploaded fine
				if (Settings.RemoveShots) {
					Log.Debug(Tag, "SmartPrintScreen: PostUpload removing the screenshot");
					Java.IO.File f = new Java.IO.File(screenshotFile);
					Log.Debug(Tag, "SmartPrintScreen: PostUpload removing the screenshot from " + f.ToString());
					if (f.Exists() && !f.IsDirectory) {
						if (
							f.Delete()
							)
							Log.Debug(Tag, "SmartPrintScreen: PostUpload removed the screenshot");
						else
							Log.Debug(Tag, "SmartPrintScreen: PostUpload failed to removed the screenshot");
					}
				} else {
					Log.Debug(Tag, "SmartPrintScreen: PostUpload not removing the screenshot");
				}
				Log.Debug(Tag, "SmartPrintScreen: PostUpload updating the screenshots list");
				Settings.ScreenshotsList += url + "\n";
				Log.Debug(Tag, "SmartPrintScreen: PostUpload notifying the UI");
				//if wi-fi is enabled then we actually failed
			} else {
				service.handler.Post(() => {
					SmartPrintScreen.MakeToast(service, Strings.AppName + ": " + Strings.ErrorCheckConnection);
				});
			}
			Log.Debug(Tag, "SmartPrintScreen: PostUpload finished");
		}

		private class StartWatchingFileObserver : FileObserver {
			private FileObserver fileObserver;
			public StartWatchingFileObserver(string path, FileObserver fileObserver) : base(path) {
				this.fileObserver = fileObserver;
			}
			public override void OnEvent(FileObserverEvents ev, string path) {
				Log.Debug(Tag, "StartWatchingFileObserver: OnEvent " + ev + " " + path);
				if ((ev & FileObserverEvents.Create) != 0 && path.Equals("Screenshots", StringComparison.InvariantCultureIgnoreCase)) {
					Log.Debug(Tag, "StartWatchingFileObserver: OnEvent " + ev + " " + path + " started watching");
					this.fileObserver.StartWatching();
				}
			}
		}
		private class UploadFileObserver : FileObserver {
			private string ssFolder;
			private SmartPrintScreen service;
			public UploadFileObserver(string path, SmartPrintScreen service) : base(path) {
				this.ssFolder = path;
				this.service = service;
			}
			public override void OnEvent(FileObserverEvents ev, string path) {
				Log.Debug(Tag, "UploadFileObserver: OnEvent " + ev + " " + path);
				if ((ev & FileObserverEvents.CloseWrite) != 0) {
					string screenshotFile = this.ssFolder + sep + path;
					Log.Debug(Tag, "UploadFileObserver: OnEvent " + ev + " at " + screenshotFile);
					if (!(new Java.IO.File(screenshotFile)).Exists()) {
		    			return;
					}
					Log.Debug(Tag, "UploadFileObserver: OnEvent " + ev + " " + path + " started bitmapping");
					SmartPrintScreen.Upload(this.service, screenshotFile);
				}
			}
		}

		private class UploadContentObserver : ContentObserver {
			private ContentResolver contentResolver;
			private SmartPrintScreen service;
			private string lastUploadingFileName = null;
			private object locker = new object();
			public UploadContentObserver(Handler ha, SmartPrintScreen service, ContentResolver contentResolver) : base(ha) {
				this.service = service;
				this.contentResolver = contentResolver;
			}
			public override void OnChange(bool selfChange, Android.Net.Uri uri) {
				lock (locker) {
					base.OnChange(selfChange, uri);
					if (uri.ToString().Contains(MediaStore.Images.Media.ExternalContentUri.ToString())) {
						ICursor cursor = null;
						try {
							cursor = contentResolver.Query(uri, new string[] {
								MediaStore.Images.Media.InterfaceConsts.DisplayName,
								MediaStore.Images.Media.InterfaceConsts.Data,
								MediaStore.Images.Media.InterfaceConsts.DateAdded,
						}, null, null, null);
							if (cursor != null && cursor.MoveToFirst()) {
								long now = Java.Lang.JavaSystem.CurrentTimeMillis() / 1000;
								do {
									string fileName = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.DisplayName));
									string path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
									long dateAdded = cursor.GetLong(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.DateAdded));
									if ((now-dateAdded) < 3
										&& this.lastUploadingFileName != fileName
										&& fileName.ToLowerInvariant().Contains("screenshot")) {
										this.lastUploadingFileName = fileName;
										SmartPrintScreen.Upload(this.service, path);
										break;
									}
								} while (cursor.MoveToNext());
							}
						} finally {
							if (cursor != null) {
								cursor.Close();
								cursor.Dispose();
							}
						}
					}
				}
			}
		}

		public static void MakeToast(Context context, string text) {
			Log.Debug(Tag, "SmartPrintScreen: MakeToast started showing toast with text " + text);
			var toast = Toast.MakeText(context, text, ToastLength.Long);
			TextView v = (TextView)toast.View.FindViewById(Android.Resource.Id.Message);
			if (v != null)
				v.Gravity = Android.Views.GravityFlags.Center;
			toast.Show();
			Log.Debug(Tag, "SmartPrintScreen: MakeToast ended showing toast");
		}
	}
}