/*
 * WordsLive - worship projection software
 * Copyright (c) 2013 Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
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
