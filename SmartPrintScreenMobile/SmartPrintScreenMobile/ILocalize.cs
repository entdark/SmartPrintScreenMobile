using System.Globalization;

namespace SmartPrintScreenMobile {
	public interface ILocalize {
		CultureInfo GetCurrentCultureInfo();
		void SetLocale(CultureInfo ci);
	}
}
