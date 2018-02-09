using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SmartPrintScreenMobile.iOS;

using UIKit;

[assembly: ExportRenderer(typeof(ViewCell), typeof(ViewCellRenderer_iOS))]
namespace SmartPrintScreenMobile.iOS {
	public class ViewCellRenderer_iOS : ViewCellRenderer {
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv) {
			var cell = base.GetCell(item, reusableCell, tv);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			return cell;
		}
	}
}