using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SmartPrintScreenMobile.iOS;

using UIKit;

[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollViewRenderer_iOS))]
namespace SmartPrintScreenMobile.iOS {
	public class ScrollViewRenderer_iOS : ScrollViewRenderer {
		protected override void OnElementChanged(VisualElementChangedEventArgs ev) {
			base.OnElementChanged(ev);

			if (ev.OldElement != null) {
			}

			if (ev.NewElement != null) {
				this.IndicatorStyle = UIScrollViewIndicatorStyle.White;
			}
		}
	}
}