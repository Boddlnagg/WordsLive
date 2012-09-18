using System;
using System.Windows.Data;

namespace WordsLive.Utils
{
	public class EnumToIndexConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var type = parameter as Type;
			if (type == null || !type.IsEnum)
				throw new ArgumentException("parameter");

			int index = 0;
			foreach(var val in type.GetEnumValues())
			{
				if (val.Equals(value))
					return index;

				index++;
			}

			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var type = parameter as Type;
			if (type == null || !type.IsEnum)
				throw new ArgumentException("parameter");

			return type.GetEnumValues().GetValue((int)value);
		}
	}
}
