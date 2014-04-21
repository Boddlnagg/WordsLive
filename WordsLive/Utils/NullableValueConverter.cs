/*
 * WordsLive - worship projection software
 * Copyright (c) 2014 Patrick Reisert
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
using System.Globalization;
using System.Windows.Data;

namespace WordsLive.Utils
{
	/// <summary>
	/// See http://jeffhandley.com/archive/2008/07/09/binding-to-nullable-values-in-xaml.aspx
	/// </summary>
	public class NullableValueConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (string.IsNullOrEmpty(value.ToString()))
				return null;

			return value;
		}

		#endregion
	}
}
