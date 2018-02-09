using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartPrintScreenMobile {
	public class SettingsItemViewModel : INotifyPropertyChanged {
		private string text = null;
		public string Text {
			get => this.text;
			set { if (this.text != value) { this.text = value; this.OnPropertyChanged(); } }
		}
		private bool isToggled = true;
		public bool IsToggled {
			get => this.isToggled;
			set { if (this.isToggled != value) { this.isToggled = value; this.OnPropertyChanged(); } }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsItemViewModel() {}
		
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
