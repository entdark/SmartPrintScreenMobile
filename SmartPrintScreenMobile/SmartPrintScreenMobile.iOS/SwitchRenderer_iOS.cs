using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SmartPrintScreenMobile.iOS;

[assembly: ExportRenderer(typeof(Switch), typeof(SwitchRenderer_iOS))]
namespace SmartPrintScreenMobile.iOS {
	public class SwitchRenderer_iOS : SwitchRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Switch> ev) {
			base.OnElementChanged(ev);

			if (ev.OldElement != null) {
			}

			if (ev.NewElement != null) {
				Control.OnTintColor = Color.FromHex("365237").ToUIColor();
				Control.ThumbTintColor = Color.FromHex("43A047").ToUIColor();
				Control.TintColor = Color.FromHex("365237").ToUIColor();
			}
		}
	}
}