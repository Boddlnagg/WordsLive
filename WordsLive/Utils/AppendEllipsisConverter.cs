using System;
using System.Windows.Data;

namespace WordsLive.Utils
{
	public class AppendEllipsisConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (parameter is string)
			{
				return (parameter as string) + "...";
			}
			else
			{
				throw new ArgumentException("parameter");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
}
