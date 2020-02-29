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
using System.ComponentModel;
using WordsLive.Core;
using WordsLive.Core.Songs.Storage;

namespace WordsLive.Songs
{
	/// <summary>
	/// Class implementing a filter for songs used in the song list.
	/// </summary>
	class SongFilter : INotifyPropertyChanged
	{
		private string keyword;

		/// <summary>
		/// Gets or sets the keyword used for keyword search.
		/// </summary>
		public string Keyword
		{
			get
			{
				return keyword;
			}
			set
			{
				keyword = value;
				NormalizedKeyword = SongData.NormalizeSearchString(keyword);
				OnPropertyChanged("Keyword");
				OnPropertyChanged("NormalizedKeyword");
			}
		}

		/// <summary>
		/// Gets the keyboard in a normalized version.
		/// </summary>
		public string NormalizedKeyword { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether a full-text-search is performed
		/// (this is an application setting).
		/// </summary>
		public bool SearchInText
		{
			get
			{
				return Properties.Settings.Default.SongListSearchInText;
			}
			set
			{
				Properties.Settings.Default.SongListSearchInText = value;
				OnPropertyChanged("SearchInText");
			}
		}

		private string source;

		/// <summary>
		/// Gets or sets the source to search for. If this is <c>null</c> or an empty string,
		/// the songs will not be filtered by source.
		/// </summary>
		public string Source
		{
			get
			{
				return source;
			}
			set
			{
				source = value;
				OnPropertyChanged("Source");
			}
		}

		private string copyright;

		/// <summary>
		/// Gets or sets the copyright to search for. If this is <c>null</c> or an empty string,
		/// the songs will not be filtered by copyright.
		/// </summary>
		public string Copyright
		{
			get
			{
				return copyright;
			}
			set
			{
				copyright = value;
				OnPropertyChanged("Copyright");
			}
		}

		private int? ccliNumber;

		/// <summary>
		/// Gets or sets the CCLI song number to search for. If this is <c>null</c>,
		/// the songs will not be filtered by CCLI song number.
		/// </summary>
		public int? CcliNumber
		{
			get
			{
				return ccliNumber;
			}
			set
			{
				ccliNumber = value;
				OnPropertyChanged("CcliNumber");
			}
		}

		/// <summary>
		/// Creates an empty song filter.
		/// </summary>
		public SongFilter()
		{
			Reset();
		}

		/// <summary>
		/// The predicate function to test a given song for a match.
		/// </summary>
		/// <param name="song">The song to test.</param>
		/// <returns><c>true</c> when the song matches the 
		/// given keyword and/or source/copyright strings and <c>false</c> otherwise.</returns>
		public bool Matches(SongData song)
		{
			// TODO: The keyword search should be able to search in source and copyright directly.
			//		 Furthermore the full text search should search for multiple independent words (ignoring their order)
			//		 and only search for an exact match if the phrase is quoted like "search phrase".
			//       This change also needs to be done in the PHP MediaServer implementation.

			if (IsEmpty)
			{
				return true;
			}

			if (!String.IsNullOrEmpty(Source))
			{
				if (!song.Sources.ContainsIgnoreCase(Source))
				{
					return false;
				}
			}

			if (!String.IsNullOrEmpty(Copyright))
			{
				if (!song.Copyright.ContainsIgnoreCase(Copyright))
				{
					return false;
				}
			}

			if (!String.IsNullOrEmpty(Keyword))
			{
				int number;
				if (song.CcliNumber.HasValue && int.TryParse(Keyword, out number))
				{
					if (song.CcliNumber.Value == number)
					{
						return true;
					}
				}
				if (!(song.SearchTitle.ContainsIgnoreCase(NormalizedKeyword) || (SearchInText && song.SearchText.ContainsIgnoreCase(NormalizedKeyword))))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Gets a value indicating whether this filter is empty (all songs will pass).
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return String.IsNullOrEmpty(Keyword) && String.IsNullOrEmpty(Source) && String.IsNullOrEmpty(Copyright);
			}
		}

		/// <summary>
		/// Resets this filter, so all songs will pass the filtering.
		/// </summary>
		public void Reset()
		{
			Keyword = "";
			Source = "";
			Copyright = "";
			CcliNumber = null;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
