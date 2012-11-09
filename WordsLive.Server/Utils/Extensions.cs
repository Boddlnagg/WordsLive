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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WordsLive.Server.Utils
{
	public static class Extensions
	{
		// GetHeader() taken from https://github.com/owin/gate/blob/master/src/Main/Gate/Headers.cs
		// Licensed under APL 2.0

		public static IEnumerable<string> GetHeaders(this IDictionary<string, IEnumerable<string>> headers,
			string name)
		{
			IEnumerable<string> value;
			return headers != null && headers.TryGetValue(name, out value) ? value : null;
		}

		public static string GetHeader(this IDictionary<string, IEnumerable<string>> headers,
			string name)
		{
			var values = GetHeaders(headers, name);
			if (values == null)
			{
				return null;
			}

			switch (values.Count())
			{
				case 0:
					return string.Empty;
				case 1:
					return values.First();
				default:
					return string.Join(",", values);
			}
		}

		public static string ComputeMD5Hash(this string str)
		{
			var md5Hasher = MD5.Create();
			return BitConverter.ToString(md5Hasher.ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "").ToLower();
		}
	}
}
