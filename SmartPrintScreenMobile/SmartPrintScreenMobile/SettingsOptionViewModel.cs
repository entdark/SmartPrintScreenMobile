using System.ComponentModel;

namespace SmartPrintScreenMobile {
	public class SettingsOptionViewModel : INotifyPropertyChanged {
		private string text = null;
		public string Text {
			get { return this.text; }
			set {
				if (this.text == value)
					return;
				this.text = value;
				OnPropertyChanged("Text");
			}
		}
		private bool isToggled = true;
		public bool IsToggled {
			get { return this.isToggled; }
			set {
				if (this.isToggled == value)
					return;
				this.isToggled = value;
				OnPropertyChanged("IsToggled");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsOptionViewModel() {}

		protected void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
