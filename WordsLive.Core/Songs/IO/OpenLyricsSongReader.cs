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
using System.IO;
using System.Xml.Linq;

namespace WordsLive.Core.Songs.IO
{
	/// <summary>
	/// See http://openlyrics.info/dataformat.html
	/// </summary>
	public class OpenLyricsSongReader : ISongReader
	{
		protected readonly XNamespace ns = "http://openlyrics.info/namespace/2009/song";

		public bool NeedsTemplate
		{
			get { return true; }
		}

		public void Read(Song song, Stream stream)
		{
			if (song == null)
				throw new ArgumentNullException("song");

			if (stream == null)
				throw new ArgumentNullException("stream");

			var doc = XDocument.Load(stream);

			if (doc.Root.Name != ns + "song")
			{
				throw new SongFormatException("File is not a valid OpenLyrics song.");
			}

			throw new NotImplementedException(); // TODO
		}
	}
}
