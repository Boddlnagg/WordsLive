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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs.Storage
{
	/// <summary>
	/// Container class for the data of a song that is relevant for the song data storages.
	/// Represents the song in a compact format.
	/// </summary>
	public class SongData
	{
		private string searchText = null;
		private string searchTitle = null;

		/// <summary>
		/// Gets or sets the song title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the relative filename (with extension).
		/// </summary>
		public string Filename { get; set; }

		/// <summary>
		/// Gets or sets the text (without chords and without the translation).
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the copyright as a single line.
		/// </summary>
		public string Copyright { get; set; }

		/// <summary>
		/// Gets or sets the sources string. This is a ';' separated list of all sources.
		/// </summary>
		public string Sources { get; set; }

		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// Gets the search text (with whitespaces and commas removed)
		/// </summary>
		[JsonIgnore]
		public string SearchText
		{
			get
			{
				if (searchText == null)
					searchText = NormalizeSearchString(Text);
				return searchText;
			}
		}

		/// <summary>
		/// Gets the search title (with whitespaces and commas removed)
		/// </summary>
		[JsonIgnore]
		public string SearchTitle
		{
			get
			{
				if (searchTitle == null)
					searchTitle = NormalizeSearchString(Title);
				return searchTitle;
			}
		}

		/// <summary>
		///  Gets the URI (using the song:// scheme) of this song.
		/// </summary>
		[JsonIgnore]
		public Uri Uri
		{
			get
			{
				return new Uri("song:///" + Filename);
			}
		}

		/// <summary>
		/// Creates a new <see cref="SongData"/> instance from a <see cref="Song"/> by
		/// extracting the relevant data.
		/// </summary>
		/// <param name="song">The song.</param>
		/// <returns>The created <see cref="SongData"/>.</returns>
		public static SongData Create(Song song)
		{
			return new SongData
			{
				Title = song.Title,
				Filename = Path.GetFileName(Uri.UnescapeDataString(song.Uri.Segments.Last())),
				Text = song.TextWithoutChords,
				Copyright = String.Join(" ", song.Copyright.Split('\n').Select(line => line.Trim())),
				Sources = String.Join("; ", song.Sources),
				Language = song.Language
			};
		}

		public static string NormalizeSearchString(string str)
		{
			return Regex.Replace(str, "([,!.:-]|\\s)+", " ");
		}
	}
}
