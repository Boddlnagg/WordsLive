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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

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

		public static Stream GetResourceStream(this Assembly assembly, string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Contains("\\"))
				throw new ArgumentException("name may not contain backslashes. Use the slash character to seperate paths.");

			return assembly.GetManifestResourceStream(Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name) + ".Resources." + name.Replace('/', '.'));
		}

		public static void ExtractResource(this Assembly assembly, string name, DirectoryInfo target)
		{
			using (var stream = assembly.GetResourceStream(name))
			{
				using (var writer = new FileStream(Path.Combine(target.FullName, name), FileMode.Create))
				{
					stream.CopyTo(writer);
				}
			}
		}

		public static string GetExtension(this Uri uri)
		{
			var s = Uri.UnescapeDataString(uri.Segments.Last()).Split('.');
			if (s.Length == 1)
				return String.Empty;
			else
				return "." + s.Last();
		}

		public static string FormatLocal(this Uri uri)
		{
			if (uri.IsFile)
				return uri.LocalPath;
			else
				return uri.AbsoluteUri;
		}

		public static NameValueCollection ParseQueryString(this Uri uri)
		{
			NameValueCollection queryParameters = new NameValueCollection();
			string[] querySegments = uri.Query.Split('&');
			foreach (string segment in querySegments)
			{
				try
				{
					string[] parts = segment.Split('=');
					if (parts.Length == 2)
					{
						string key = parts[0].Trim(new char[] { '?', ' ' });
						string val = Uri.UnescapeDataString(parts[1].Trim());

						queryParameters.Add(key, val);
					}
				}
				catch { }
			}

			return queryParameters;
		}
	}
}
