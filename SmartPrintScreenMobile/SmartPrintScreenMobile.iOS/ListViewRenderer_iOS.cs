using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SmartPrintScreenMobile.iOS;

using UIKit;

[assembly: ExportRenderer(typeof(ListView), typeof(ListViewRenderer_iOS))]
namespace SmartPrintScreenMobile.iOS {
	public class ListViewRenderer_iOS : ListViewRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<ListView> ev) {
			base.OnElementChanged(ev);

			if (ev.OldElement != null) {
			}

			if (ev.NewElement != null) {
				Control.IndicatorStyle = UIScrollViewIndicatorStyle.White;
				Control.TableFooterView = new UIView();
			}
		}
	}
}