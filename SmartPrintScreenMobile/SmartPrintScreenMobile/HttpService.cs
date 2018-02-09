using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmartPrintScreenMobile {
	public static class HttpService {
		public const int TimeOut = 13337;

		private static readonly HttpClient client;

		static HttpService() {
			HttpService.client = new HttpClient();
		}

		public static async Task<Stream> GetStream(string uri, CancellationToken token) {
			try {
				var getAsyncTask = HttpService.client.GetAsync(uri, token);
				if (await Task.WhenAny(getAsyncTask, Task.Delay(TimeOut)).ConfigureAwait(false) == getAsyncTask) {
					using (var response = await getAsyncTask.ConfigureAwait(false)) {
						if (response.IsSuccessStatusCode) {
							return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
						}
						return null;
					}
				}
			} catch {}
			return null;
		}

		public static async Task<string> UploadToImgur(byte []image) {
			using (var source = new CancellationTokenSource())
			try {
				using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image")) {
					request.Headers.TryAddWithoutValidation("Authorization", "Client-ID " + APIKeys.ImgurClientID);
					string base64Image = Convert.ToBase64String(image);
					var content = new StringContent(base64Image, Encoding.UTF8, "text/plain");
					request.Content = content;
					var sendAsyncTask = HttpService.client.SendAsync(request, source.Token);
					if (await Task.WhenAny(sendAsyncTask, Task.Delay(TimeOut)) == sendAsyncTask) {
						using (var response = await sendAsyncTask) {
							if (response.IsSuccessStatusCode) {
								string result = await response.Content.ReadAsStringAsync();
								Regex reg = new Regex("link\":\"(.*?)\"");
								Match match = reg.Match(result);
								string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
								return url;
							}
							return null;
						}
					} else {
						source.Cancel();
					}
				}
			} catch (Exception exception) {
				source.Cancel();
			}
			return null;
		}
	}
}
