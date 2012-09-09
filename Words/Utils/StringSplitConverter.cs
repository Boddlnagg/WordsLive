using System;
using System.Windows.Data;

namespace Words.Utils
{
	public class StringSplitConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var str = (string)value;
			var delimiter = (string)parameter;
			return str.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
