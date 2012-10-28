using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WordsLive.Editor
{
	public class LanguageToLanguageCodeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string name = (string)value;

			if (String.IsNullOrEmpty(name))
				return String.Empty;

			int index = name.IndexOfAny(new char[] { '/', ' ', '(' });
			if (index > 0)
			{
				name = name.Substring(0, index);
			}

			switch (name.ToLower())
			{
				case "deutsch":
				case "german":
					return "de";
				case "schweizerdeutsch":
					return "de-ch";
				case "englisch":
				case "english":
					return "en";
				case "italienisch":
				case "italiano":
				case "italian":
					return "it";
				case "französisch":
				case "francais":
				case "french":
					return "fr";
				case "spanisch":
				case "espanol":
				case "spanish":
					return "es";
				default:
					return String.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
