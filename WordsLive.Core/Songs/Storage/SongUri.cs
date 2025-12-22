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

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Methods for handling "song://" URIs.
	/// </summary>
	public class SongUri
	{
		/// <summary>
		/// Returns a "song://" URI for the given filename within the song storage folder.
		/// </summary>
		/// <param name="filename">The filename. Include the subfolder when the file is in a subfolder of the song storage folder.</param>
		/// <returns>The "song://" URI with encoded special characters if needed.</returns>
		public static Uri GetUri(string filename)
		{
			return new Uri("song:///" + Uri.EscapeDataString(filename));
		}

		/// <summary>
		/// Extracts the filename within the song storage folder from a "song://" URI.
		/// </summary>
		/// <param name="uri">The "song://" URI.</param>
		/// <returns>The filename relative to the song storage folder. If the file is in a subfolder, then this includes the subfolder and the used delimiter is "/".</returns>
		public static string GetFilename(Uri uri)
		{
			if (uri.Scheme != "song")
				throw new ArgumentException("uri");

			return Uri.UnescapeDataString(uri.AbsolutePath).Substring(1);
		}
	}
}
