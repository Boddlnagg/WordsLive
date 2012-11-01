/*
 * WordsLive - worship projection software
 * Copyright (c) 2012 Patrick Reisert
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

namespace WordsLive.Core
{
	/// <summary>
	/// Contains some extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns a value indicating whether a given substring occurs in the string, ignoring the case.
		/// </summary>
		/// <param name="str">The string to search in.</param>
		/// <param name="value">The substring to search for.</param>
		/// <returns>
		///   <c>true</c> if the string contains the specified substring; otherwise, <c>false</c>.
		/// </returns>
		public static bool ContainsIgnoreCase(this string str, string value)
		{
			return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
