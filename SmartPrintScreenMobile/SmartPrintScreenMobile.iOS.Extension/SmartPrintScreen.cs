using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using MobileCoreServices;
using Social;
using UIKit;

namespace SmartPrintScreenMobile.iOS.Extension {
	public partial class SmartPrintScreen : SLComposeServiceViewController {
		public SmartPrintScreen(IntPtr handle) : base(handle) {
			this.Localize();
		}

		public override void DidReceiveMemoryWarning() {
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void LoadView() {
			this.View = new UIView();
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();
		}

		public override bool IsContentValid() {
			if (ExtensionContext.InputItems.Length != 1)
				return false;

			var imageItem = ExtensionContext.InputItems[0];
			if (imageItem == null)
				return false;

			if (imageItem.Attachments.Length != 1)
				return false;

			// Verify that we have a valid NSItemProvider
			var imageItemProvider = imageItem.Attachments[0];
			if (imageItemProvider == null)
				return false;

			// Look for an image inside the NSItemProvider
			if (!imageItemProvider.HasItemConformingTo(UTType.Image))
				return false;

			return true;
		}

		public override async void ViewDidAppear(bool animated) {
			base.ViewDidAppear(animated);

			var imageItem = ExtensionContext.InputItems[0];
			if (imageItem == null) {
				await this.FormatErrorAlert();
				return;
			}
			var imageItemProvider = imageItem.Attachments[0];
			if (imageItemProvider == null) {
				await this.FormatErrorAlert();
				return;
			}
			if (!imageItemProvider.HasItemConformingTo(UTType.Image)) {
				await this.FormatErrorAlert();
				return;
			}
			UIImage image = null;
			var obj = await imageItemProvider.LoadItemAsync(UTType.Image, null);
			// This is true when you call extension from Photo's ActivityViewController
			if (obj is NSUrl nsUrl) {
				image = UIImage.LoadFromData(NSData.FromUrl(nsUrl));
			}
			// This is true when you call extension from Main App
			if (image == null)
				image = obj as UIImage;

			var source = new CancellationTokenSource();
			var alert = UIAlertController.Create(Strings.Uploading, null, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create(Strings.Cancel, UIAlertActionStyle.Default, (action) => {
				source.Cancel();
				ExtensionContext.CompleteRequest(null, null);
			}));
			await PresentViewControllerAsync(alert, true);
			string url = await this.UploadToImgur(image, source.Token);
			await DismissViewControllerAsync(true);
			source.Dispose();
			alert = UIAlertController.Create(
				null,
				url != null ?
				Strings.CopiedToClipboardToast + ":\n" + url :
				Strings.AppName + ": " + Strings.ErrorCheckConnection,
				UIAlertControllerStyle.Alert
			);
			alert.AddAction(UIAlertAction.Create(Strings.Ok, UIAlertActionStyle.Default, (action) => {
				if (url != null) {
					Plugin.Clipboard.CrossClipboard.Current.SetText(url);
					var userDefaults = new NSUserDefaults("group.com.vlbor.SmartPrintScreen", NSUserDefaultsType.SuiteName);
					string screenshotsList = userDefaults.StringForKey(Shared.ScreenshotsListKey);
					if (string.IsNullOrEmpty(screenshotsList))
						screenshotsList = "";
					screenshotsList += url + "\n";
					userDefaults.SetString(screenshotsList, Shared.ScreenshotsListKey);
				}
				// Inform the host that we're done, so it un-blocks its UI. Note: Alternatively you could call super's -didSelectPost, which will similarly complete the extension context.
				ExtensionContext.CompleteRequest(null, null);
			}));
			await PresentViewControllerAsync(alert, true);
		}

		private async Task FormatErrorAlert() {
			var alert = UIAlertController.Create(Strings.ErrorFormat, null, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create(Strings.Ok, UIAlertActionStyle.Default, (action) => {
				ExtensionContext.CompleteRequest(null, null);
			}));
			await PresentViewControllerAsync(alert, true);
		}

		private async Task<string> UploadToImgur(UIImage image, CancellationToken token) {
			try {
				NSUrlSession session;
				session = NSUrlSession.FromConfiguration(
					NSUrlSessionConfiguration.EphemeralSessionConfiguration,
					new NSUrlSessionTaskDelegate() as INSUrlSessionDelegate,
					new NSOperationQueue()
				);
				NSUrl uploadHandleUrl = NSUrl.FromString("https://api.imgur.com/3/image");
				NSMutableUrlRequest request = new NSMutableUrlRequest(uploadHandleUrl) {
					HttpMethod = "POST",
					Headers = NSDictionary.FromObjectsAndKeys(
						new []{ "Client-ID " + APIKeys.ImgurClientID },
						new []{ "Authorization" }
					)
				};
				request["Content-Type"] = "text/paint";
				var png = image.AsPNG();
				string base64Image = png?.GetBase64EncodedString(NSDataBase64EncodingOptions.SixtyFourCharacterLineLength);
				request.Body = $"{base64Image}";
				var dataTask = session.CreateDataTaskAsync(request);
				var dataTaskCancellable = Task.Run(async () => await dataTask, token);
				if (await Task.WhenAny(dataTaskCancellable, Task.Delay(HttpService.TimeOut)) == dataTaskCancellable) {
					var dataTaskRequest = await dataTaskCancellable;
					string result = new NSString(dataTaskRequest.Data, NSStringEncoding.UTF8);
					Regex reg = new Regex("link\":\"(.*?)\"");
					Match match = reg.Match(result);
					return match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
				}
			} catch {
			}
			return null;
		}

		private void Localize() {
			var ci = this.GetCurrentCultureInfo();
			Strings.Culture = ci; // set the RESX for resource localization
			this.SetLocale(ci); // set the Thread for locale-aware methods
		}
		private void SetLocale(CultureInfo ci) {
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}
		private CultureInfo GetCurrentCultureInfo() {
			var netLanguage = "en";
			if (NSLocale.PreferredLanguages.Length > 0) {
				var pref = NSLocale.PreferredLanguages[0];
				netLanguage = iOSToDotnetLanguage(pref);
			}
			// this gets called a lot - try/catch can be expensive so consider caching or something
			CultureInfo ci = null;
			try {
				ci = new CultureInfo(netLanguage);
			} catch (CultureNotFoundException e1) {
				// iOS locale not valid .NET culture (eg. "en-ES" : English in Spain)
				// fallback to first characters, in this case "en"
				try {
					var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
					ci = new CultureInfo(fallback);
				} catch (CultureNotFoundException e2) {
					// iOS language not valid .NET culture, falling back to English
					ci = new CultureInfo("en");
				}
			}
			return ci;
		}
		private string iOSToDotnetLanguage(string iOSLanguage) {
			var netLanguage = iOSLanguage;
			//certain languages need to be converted to CultureInfo equivalent
			switch (iOSLanguage) {
			case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
			case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
				netLanguage = "ms"; // closest supported
				break;
			case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
				netLanguage = "de-CH"; // closest supported
				break;
				// add more application-specific cases here (if required)
				// ONLY use cultures that have been tested and known to work
			}
			return netLanguage;
		}
		private string ToDotnetFallbackLanguage(PlatformCulture platCulture) {
			var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);
			switch (platCulture.LanguageCode) {
			case "pt":
				netLanguage = "pt-PT"; // fallback to Portuguese (Portugal)
				break;
			case "gsw":
				netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
				break;
				// add more application-specific cases here (if required)
				// ONLY use cultures that have been tested and known to work
			}
			return netLanguage;
		}
	}
}
