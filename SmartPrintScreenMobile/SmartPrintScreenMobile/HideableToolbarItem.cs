using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace SmartPrintScreenMobile {
	public class HideableToolbarItem : ToolbarItem {
		public HideableToolbarItem() {}
		
		public bool IsVisible {
			get { return (bool)this.GetValue(IsVisibleProperty); }
			set { this.SetValue(IsVisibleProperty, value); }
		}

		public static BindableProperty IsVisibleProperty =
			BindableProperty.Create("IsVisible", typeof(bool), typeof(HideableToolbarItem), true);

		protected override void OnPropertyChanged(string propertyName) {
			base.OnPropertyChanged(propertyName);
			if (propertyName == HideableToolbarItem.IsVisibleProperty.PropertyName) {
				this.OnIsVisibleChanged(this.IsVisible);
			}
		}

		private void OnIsVisibleChanged(bool newvalue) {
			var parent = this.Parent as ContentPage;

			if (parent == null)
				return;

			var items = parent.ToolbarItems;

			if (newvalue && !items.Contains(this)) {
				items.Add(this);
			} else if (!newvalue && items.Contains(this)) {
				items.Remove(this);
			}
		}
	}
}