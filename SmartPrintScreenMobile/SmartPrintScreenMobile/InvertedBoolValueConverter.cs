using System;
using System.Globalization;
using Xamarin.Forms;

namespace SmartPrintScreenMobile {
	public class InvertedBoolValueConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return true;
			return !(bool)value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null)
				return true;
			return !(bool)value;
		}
	}
}
