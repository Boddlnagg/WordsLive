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

namespace WordsLive.Utils
{
	/// <summary>
	/// Two-way converter that converts an enum value to it's index in the enum specified as parameter.
	/// </summary>
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
