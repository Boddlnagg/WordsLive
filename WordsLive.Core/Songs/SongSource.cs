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
using System.ComponentModel;
using Newtonsoft.Json;

namespace WordsLive.Core.Songs
{
	/// <summary>
	/// Represents the source information of a song.
	/// </summary>
	[JsonConverter(typeof(Json.JsonSongSourceConverter))]
	public class SongSource : ISongElement, INotifyPropertyChanged
	{
		private string songbook;
		private int? number;

		/// <summary>
		/// Gets or sets the songbook.
		/// </summary>
		public string Songbook
		{
			get
			{
				return songbook;
			}
			set
			{
				Undo.ChangeFactory.OnChangingTryMerge(this, "Songbook", songbook, value);
				songbook = value;
				OnPropertyChanged("Songbook");
			}
		}

		/// <summary>
		/// Gets or sets the number of the song in the songbook (<c>null</c> if unknown)
		/// </summary>
		public int? Number
		{
			get
			{
				return number;
			}
			set
			{
				if (value != number)
				{
					Undo.ChangeFactory.OnChangingTryMerge(this, "Number", number, value);
					number = value;
					OnPropertyChanged("Number");
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SongSource"/> class.
		/// </summary>
		/// <param name="root">The song this source belongs to.</param>
		public SongSource(Song root)
		{
			this.Root = root;
		}
		
		/// <summary>
		/// Generated a <see cref="SongSource"/> by parsing a string.
		/// A variety of formats is supported, e.g. '[Book] / [Nr]' or '[Book], Nr. [Nr]'.
		/// </summary>
		/// <param name="source">The string to parse.</param>
		/// <returns>The parsed source object.</returns>
		public static SongSource Parse(string source, Song root)
		{
			SongSource result = new SongSource(root);
			
			if(String.IsNullOrWhiteSpace(source))
				return result;
			
			bool success = false;
			int n;
			
			if (source.Contains("/"))
			{
				var parts = source.Split('/');
				result.songbook = parts[0].Trim();
				if (int.TryParse(parts[1].Trim(), out n))
				{
					result.number = n;
					success = true;
				}
			}
	
			if (!success)
			{
				int index = source.LastIndexOfAny(new char[] {' ','.',','});
				if (index >= 0 && int.TryParse(source.Substring(index+1).Trim(), out n))
				{
					result.Number = n;
					string book = source.Substring(0, index+1).Trim();
					if (book.EndsWith("Nr."))
						book = book.Substring(0, book.Length - 3).Trim();
					if (book.EndsWith(","))
						book = book.Substring(0, book.Length - 1).Trim();
					result.songbook = book;
				}
				else
				{
					result.songbook = source;
				}
			}
			
			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance in the format '[Book] / [Nr]'.
		/// If the <see cref="Number"/> is <c>null</c> or <c>0</c> only the <see cref="Songbook"/> name is returned.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(Songbook))
				return string.Empty;
			else if (!Number.HasValue || Number.Value == 0)
				return Songbook;
			else
				return Songbook + " / " + Number;
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		[JsonIgnore]
		public Song Root { get; private set; }

		#endregion
	}
}