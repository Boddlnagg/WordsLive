using System;
using System.Windows.Data;

namespace WordsLive.Utils
{
	public class RemoveAccessKeyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (parameter is string)
			{
				var str = parameter as string;
				var i = str.IndexOf('_');
				if (i >= 0)
					return str.Remove(i, 1);
				else
					return str;
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
